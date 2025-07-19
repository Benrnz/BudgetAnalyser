using System.Windows.Threading;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Statement;

[AutoRegisterWithIoC(SingleInstance = true)]
public class TransactionsControllerFileOperations : ControllerBase
{
    // TODO Direct controller references are not ideal.
    private readonly LoadFileController loadFileController;
    private readonly IUserMessageBox messageBox;
    private readonly ITransactionManagerService transactionService;
    private bool doNotUseLoadingData;

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
        ViewModel = new TransactionsViewModel(applicationDatabaseService, this.transactionService);
    }

    public bool LoadingData
    {
        get => this.doNotUseLoadingData;
        private set
        {
            if (this.doNotUseLoadingData == value)
            {
                return;
            }

            this.doNotUseLoadingData = value;
            OnPropertyChanged();
        }
    }

    internal TransactionsViewModel ViewModel { get; }

    internal void Close()
    {
        ViewModel.TransactionsModel = null;
        NotifyOfReset();
        ViewModel.TriggerRefreshTotalsRow();
        Messenger.Send(new TransactionSetModelReadyMessage(null));
    }

    internal async Task MergeInNewTransactions()
    {
        PersistenceOperationCommands.SaveDatabaseCommand.Execute(this);

        var fileName = await GetFileNameFromUser();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            // User cancelled
            return;
        }

        try
        {
            var account = this.loadFileController.SelectedExistingAccountName ?? throw new NotSupportedException("No Account selected to import transactions into.");
            await this.transactionService.ImportAndMergeBankStatementAsync(fileName, account);

            await SyncWithServiceAsync();
            Messenger.Send(new TransactionsChangedMessage());
            OnPropertyChanged(nameof(ViewModel));
            NotifyOfEdit();
            ViewModel.TriggerRefreshTotalsRow();
            Messenger.Send(new TransactionSetModelReadyMessage(ViewModel.TransactionsModel));
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
        Messenger.Send(new TransactionSetModelHasBeenModifiedMessage());
    }

    internal async Task SyncWithServiceAsync()
    {
        var transactionsModel = this.transactionService.TransactionSetModel;
        ViewModel.TransactionsModel = null; // Prevent events from firing while updating the model.
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

                ViewModel.TransactionsModel = transactionsModel;
                ViewModel.TriggerRefreshTotalsRow();

                Messenger.Send(new TransactionSetModelReadyMessage(ViewModel.TransactionsModel));

                LoadingData = false;
            });
    }

    private void FileCannotBeLoaded(Exception ex)
    {
        this.messageBox.Show("The file was not loaded.\n" + ex.Message);
    }

    /// <summary>
    ///     Prompts the user for a filename and other required parameters to be able to merge the statement file.
    /// </summary>
    /// <returns>
    ///     The user selected filename. All other required parameters are accessible from the
    ///     <see cref="LoadFileController" />.
    /// </returns>
    private async Task<string> GetFileNameFromUser()
    {
        var transactionsModel = ViewModel.TransactionsModel ?? throw new InvalidOperationException("Transactions Set Model is null, uninitialised or not loaded.");
        await this.loadFileController.RequestUserInputForMerging(transactionsModel);

        return this.loadFileController.FileName ?? string.Empty;
    }

    private void NotifyOfReset()
    {
        if (ViewModel is { Dirty: false, TransactionsModel: null })
        {
            // No need to notify of reset if the statement is already null. This happens during first load before the statement is loaded
            return;
        }

        ViewModel.Dirty = false;
        Messenger.Send(new TransactionSetModelHasBeenModifiedMessage());
    }
}
