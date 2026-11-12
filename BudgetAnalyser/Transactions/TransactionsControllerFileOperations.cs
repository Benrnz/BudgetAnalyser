using System.Windows.Threading;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Filtering;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Transactions;

[AutoRegisterWithIoC(SingleInstance = true)]
public partial class TransactionsControllerFileOperations : ControllerBase, ITransactionsControllerFileOperations
{
    private readonly BankFeedImportController bankFeedImportController;
    private readonly LoadFileController loadFileController;
    private readonly IUserMessageBox messageBox;
    private readonly ITransactionManagerService transactionService;
    private CancellationTokenSource? bankFeedDownloadCancellation;

    public TransactionsControllerFileOperations(
        IMessenger messenger,
        UserPrompts userPrompts,
        LoadFileController loadFileController,
        IApplicationDatabaseFacade applicationDatabaseService,
        ITransactionManagerService transactionManagerService,
        BankFeedImportController bankFeedImportController)
        : base(messenger)
    {
        if (applicationDatabaseService is null)
        {
            throw new ArgumentNullException(nameof(applicationDatabaseService));
        }

        this.messageBox = userPrompts.MessageBox ?? throw new ArgumentNullException(nameof(userPrompts.MessageBox));
        this.loadFileController = loadFileController ?? throw new ArgumentNullException(nameof(loadFileController));
        this.transactionService = transactionManagerService ?? throw new ArgumentNullException(nameof(transactionManagerService));
        this.bankFeedImportController = bankFeedImportController ?? throw new ArgumentNullException(nameof(bankFeedImportController));
        ViewModel = new TransactionsListViewModel(applicationDatabaseService, this.transactionService);
        CancelBankFeedDownloadCommand = new RelayCommand(OnCancelBankFeedDownload, () => IsDownloadingFromBankFeed);
    }

    public IRelayCommand CancelBankFeedDownloadCommand { get; }

    public TransactionsListViewModel ViewModel { get; }

    [ObservableProperty]
    public partial bool IsDownloadingFromBankFeed { get; private set; }

    [ObservableProperty]
    public partial bool LoadingData { get; private set; }

    public void NotifyOfEdit()
    {
        ViewModel.Dirty = true;
        Messenger.Send(new TransactionsListModelHasBeenModifiedMessage());
    }

    public async Task SyncWithServiceAsync()
    {
        var transactionList = this.transactionService.TransactionsListModel;
        ViewModel.TransactionsList = null; // Prevent events from firing while updating the model.
        LoadingData = true;
        await Dispatcher.BeginInvoke(
            DispatcherPriority.Normal,
            () =>
            {
                // Update all UI bound properties.
                var requestCurrentFilterMessage = new RequestFilterMessage(this);
                Messenger.Send(requestCurrentFilterMessage);
                if (requestCurrentFilterMessage.Criteria is not null)
                {
                    this.transactionService.FilterTransactions(requestCurrentFilterMessage.Criteria);
                }

                // Ensures initial first time load triggers load of transactions.
                ViewModel.TransactionsList = transactionList;
                ViewModel.TriggerRefreshTotalsRow();

                // Triggers all UI elements to update
                Messenger.Send(new FilterAppliedMessage(this, requestCurrentFilterMessage.Criteria ?? new GlobalFilterCriteria()));

                Messenger.Send(new TransactionsListModelReadyMessage(ViewModel.TransactionsList));

                LoadingData = false;
            });
    }

    public void Close()
    {
        ViewModel.TransactionsList = null;
        NotifyOfReset();
        ViewModel.TriggerRefreshTotalsRow();
        Messenger.Send(new TransactionsListModelReadyMessage(null));
    }

    public async Task MergeInBankFeedTransactions()
    {
        await PersistenceOperationCommands.SaveDatabaseCommand.ExecuteAsync(this);

        var transactions = ViewModel.TransactionsList ?? throw new InvalidOperationException("Transactions Model is null, uninitialised or not loaded.");
        var request = await this.bankFeedImportController.RequestUserInputAsync(transactions, DateOnlyExt.Today());
        if (request is null)
        {
            // User cancelled
            return;
        }

        string outcome;
        using var cancellation = new CancellationTokenSource();
        this.bankFeedDownloadCancellation = cancellation;
        LoadingData = true;
        IsDownloadingFromBankFeed = true;
        try
        {
            var result = await this.transactionService.ImportAndMergeBankFeedTransactionsAsync(request.Range, request.Mappings, cancellation.Token);

            // The download has been merged into the transactions list, so it is too late to cancel.
            IsDownloadingFromBankFeed = false;
            if (result.TotalImported > 0)
            {
                await SyncWithServiceAsync();
                Messenger.Send(new TransactionsChangedMessage());
                OnPropertyChanged(nameof(ViewModel));
                NotifyOfEdit();
                ViewModel.TriggerRefreshTotalsRow();
                Messenger.Send(new TransactionsListModelReadyMessage(ViewModel.TransactionsList));
            }

            outcome = BankFeedImportSummary.Describe(result);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // Nothing is merged until every account has been downloaded, so cancelling part way through leaves the transactions list unchanged.
            outcome = "Download cancelled. Nothing was imported.";
        }
        catch (BankFeedAuthenticationException)
        {
            outcome = "Your bank feed credentials were rejected. Update them on the Settings tab. Nothing was imported.";
        }
        catch (BankFeedException ex)
        {
            // Most bank feed messages already end with a full stop, some (for example an error passed on from the feed) do not.
            outcome = $"Download failed: {ex.Message.TrimEnd().TrimEnd('.')}. Nothing was imported.";
        }
        finally
        {
            // Always cleared, including when an unexpected exception is thrown, so the view is never left showing as busy.
            this.bankFeedDownloadCancellation = null;
            IsDownloadingFromBankFeed = false;
            LoadingData = false;
        }

        // Shown after the busy indicator has been cleared.
        this.messageBox.Show(outcome, "Download from bank feed");
    }

    public async Task MergeInNewTransactions()
    {
        await PersistenceOperationCommands.SaveDatabaseCommand.ExecuteAsync(this);

        var fileName = await GetFileNameFromUser();
        if (string.IsNullOrWhiteSpace(fileName) || this.loadFileController.SelectedExistingAccountName is null)
        {
            // User cancelled
            return;
        }

        try
        {
            var account = this.loadFileController.SelectedExistingAccountName;
            await this.transactionService.ImportAndMergeTransactionsExtractAsync(fileName, account);

            await SyncWithServiceAsync();
            Messenger.Send(new TransactionsChangedMessage());
            OnPropertyChanged(nameof(ViewModel));
            NotifyOfEdit();
            ViewModel.TriggerRefreshTotalsRow();
            Messenger.Send(new TransactionsListModelReadyMessage(ViewModel.TransactionsList));
        }
        catch (NotSupportedException ex)
        {
            FileCannotBeLoaded(ex);
        }
        catch (KeyNotFoundException ex)
        {
            FileCannotBeLoaded(ex);
        }
        catch (TransactionsAlreadyImportedException ex)
        {
            FileCannotBeLoaded(ex);
        }
        finally
        {
            this.loadFileController.Reset();
        }
    }

    private void OnCancelBankFeedDownload()
    {
        this.bankFeedDownloadCancellation?.Cancel();
    }

    partial void OnIsDownloadingFromBankFeedChanged(bool value)
    {
        CancelBankFeedDownloadCommand.NotifyCanExecuteChanged();
    }

    private void FileCannotBeLoaded(Exception ex)
    {
        this.messageBox.Show("The file was not loaded.\n" + ex.Message);
    }

    /// <summary>
    ///     Prompts the user for a filename and other required parameters to be able to merge the transactions extract.
    /// </summary>
    /// <returns>
    ///     The user selected filename. All other required parameters are accessible from the
    ///     <see cref="LoadFileController" />.
    /// </returns>
    private async Task<string> GetFileNameFromUser()
    {
        var transactions = ViewModel.TransactionsList ?? throw new InvalidOperationException("Transactions Model is null, uninitialised or not loaded.");
        await this.loadFileController.RequestUserInputForMerging(transactions);

        return this.loadFileController.FileName ?? string.Empty;
    }

    private void NotifyOfReset()
    {
        if (ViewModel is { Dirty: false, TransactionsList: null })
        {
            // No need to notify of reset if the transactions list is already null. This happens during first load before the transactions list is loaded
            return;
        }

        ViewModel.Dirty = false;
        Messenger.Send(new TransactionsListModelHasBeenModifiedMessage());
    }
}
