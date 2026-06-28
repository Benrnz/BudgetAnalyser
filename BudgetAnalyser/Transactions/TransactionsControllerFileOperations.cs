using System.Windows.Threading;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Filtering;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Transactions;

[AutoRegisterWithIoC(SingleInstance = true)]
public class TransactionsControllerFileOperations : ControllerBase
{
    private readonly LoadFileController loadFileController;
    private readonly IUserMessageBox messageBox;
    private readonly ITransactionManagerService transactionService;

    public TransactionsControllerFileOperations(
        IUiContext uiContext,
        LoadFileController loadFileController,
        IApplicationDatabaseFacade applicationDatabaseService,
        ITransactionManagerService transactionManagerService)
        : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        if (applicationDatabaseService is null)
        {
            throw new ArgumentNullException(nameof(applicationDatabaseService));
        }

        this.messageBox = uiContext.UserPrompts.MessageBox;
        this.loadFileController = loadFileController ?? throw new ArgumentNullException(nameof(loadFileController));
        this.transactionService = transactionManagerService ?? throw new ArgumentNullException(nameof(transactionManagerService));
        ViewModel = new TransactionsListViewModel(applicationDatabaseService, this.transactionService);
    }

    public bool LoadingData
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    internal TransactionsListViewModel ViewModel { get; }

    internal void Close()
    {
        ViewModel.TransactionsList = null;
        NotifyOfReset();
        ViewModel.TriggerRefreshTotalsRow();
        Messenger.Send(new TransactionsListModelReadyMessage(null));
    }

    internal async Task MergeInNewTransactions()
    {
        PersistenceOperationCommands.SaveDatabaseCommand.Execute(this);

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

    internal void NotifyOfEdit()
    {
        ViewModel.Dirty = true;
        Messenger.Send(new TransactionsListModelHasBeenModifiedMessage());
    }

    internal async Task SyncWithServiceAsync()
    {
        var transactionList = this.transactionService.TransactionsListModel;
        ViewModel.TransactionsList = null; // Prevent events from firing while updating the model.
        LoadingData = true;
        await Dispatcher.CurrentDispatcher.BeginInvoke(
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
