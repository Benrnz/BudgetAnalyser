using System.Collections.ObjectModel;
using System.ComponentModel;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Transactions;

[AutoRegisterWithIoC(SingleInstance = true)]
public class TopTransactionsListController : ControllerBase, IShowableController
{
    private readonly ILogger logger;
    private readonly ITransactionManagerService transactionService;
    private Guid shellDialogCorrelationId;

    public TopTransactionsListController(
        IMessenger messenger,
        ILogger logger,
        AppliedRulesController appliedRulesController,
        EditRulesController editRulesController,
        EditingTransactionController editingTransactionController,
        SplitTransactionController splitTransactionController,
        TransactionsControllerFileOperations fileOperations,
        ITransactionManagerService transactionService)
        : base(messenger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        FileOperations = fileOperations ?? throw new ArgumentNullException(nameof(fileOperations));
        AppliedRulesController = appliedRulesController ?? throw new ArgumentNullException(nameof(appliedRulesController));
        EditRulesController = editRulesController ?? throw new ArgumentNullException(nameof(editRulesController));
        EditingTransactionController = editingTransactionController ?? throw new ArgumentNullException(nameof(editingTransactionController));
        SplitTransactionController = splitTransactionController ?? throw new ArgumentNullException(nameof(splitTransactionController));

        this.transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));

        ClearSearchCommand = new RelayCommand(ClearSearch);
        EditTransactionCommand = new RelayCommand(OnEditTransactionCommandExecute, ViewModel.HasSelectedRow);
        MergeBankExtractCommand = new RelayCommand(OnMergeExtractCommandExecute);
        NavigateNextPageCommand = new RelayCommand(NavigateNextPage, () => CanNavigateNext);
        NavigatePreviousPageCommand = new RelayCommand(NavigatePreviousPage, () => CanNavigatePrevious);
        SplitTransactionCommand = new RelayCommand(OnSplitTransactionCommandExecute, ViewModel.HasSelectedRow);

        // When SelectedRow changes need to check Command CanExecute's
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        Messenger.Register<TopTransactionsListController, FilterAppliedMessage>(this, static (r, m) => r.OnGlobalDateFilterApplied(m));
        Messenger.Register<TopTransactionsListController, BudgetReadyMessage>(this, static (r, m) => r.OnBudgetReadyMessageReceived(m));
        Messenger.Register<TopTransactionsListController, ShellDialogResponseMessage>(this, OnShellDialogResponseMessageReceived);

        this.transactionService.Closed += OnClosedNotificationReceived;
        this.transactionService.NewDataSourceAvailable += OnNewDataSourceAvailableNotificationReceived;
        this.transactionService.Saved += OnSavedNotificationReceived;
    }

    public AppliedRulesController AppliedRulesController { get; }

    /// <summary>
    ///     Gets or sets the bucket filter. This is a string filter on the bucket code plus blank for all, and "[Uncatergorised]" for anything without a bucket.
    ///     Only relevant when the view is displaying transactions by date.  The filter is hidden when shown in GroupByBucket mode.
    /// </summary>
    public string? BucketFilter
    {
        get;

        set
        {
            if (Equals(value, field))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            ViewModel.Transactions = this.transactionService.FilterByBucket(BucketFilter);
            ViewModel.TriggerRefreshTotalsRow();
            CurrentPage = 1;
        }
    }

    public bool CanNavigateNext
    {
        get;
        private set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool CanNavigatePrevious
    {
        get;
        private set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public IRelayCommand ClearSearchCommand { get; }

    public int CurrentPage
    {
        get;
        set
        {
            field = value;
            UpdatePagedTransactions();
            OnPropertyChanged();
        }
    } = 1;

    public EditingTransactionController EditingTransactionController { get; }
    public EditRulesController EditRulesController { get; }
    public IRelayCommand EditTransactionCommand { get; }
    public TransactionsControllerFileOperations FileOperations { get; }

    public IRelayCommand MergeBankExtractCommand { get; }

    public IRelayCommand NavigateNextPageCommand { get; }

    public IRelayCommand NavigatePreviousPageCommand { get; }

    public int PageSize
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            if (value < 1)
            {
                value = 10;
            }

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalPages));
        }
    } = 10;

    public IRelayCommand SplitTransactionCommand { get; }

    internal SplitTransactionController SplitTransactionController { get; }

    public string? TextFilter
    {
        get;
        set
        {
            if (Equals(value, field))
            {
                return;
            }

            field = string.IsNullOrEmpty(value) ? null : value;
            OnPropertyChanged();
            ViewModel.Transactions = this.transactionService.FilterBySearchText(TextFilter);
            ViewModel.TriggerRefreshTotalsRow();
            CurrentPage = 1;
        }
    }

    public int TotalPages => (ViewModel.Transactions.Count + PageSize - 1) / PageSize;

    public TransactionsListViewModel ViewModel => FileOperations.ViewModel;

    public bool Shown
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public void ClearSearch()
    {
        TextFilter = null;
        ViewModel.Transactions = this.transactionService.ClearBucketAndTextFilters();
        UpdateCommandCanExecute();
    }

    public void NavigateNextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
        }
    }

    public void NavigatePreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }

    public void RegisterListener<TMessage>(TopTransactionsUserControl recipient, MessageHandler<TopTransactionsUserControl, TMessage> handler) where TMessage : MessageBase
    {
        Messenger.Register(recipient, handler);
    }

    private async Task CheckBudgetContainsAllUsedBucketsFromTransactions(BudgetCollection? budgets = null)
    {
        if (!await this.transactionService.ValidateWithCurrentBudgetsAsync(budgets))
        {
            // In theory should not occur.  When loading transactions from the BAX CSV file, transactions are created using the already loaded budget, and this process will throw if a bucket doesn't
            // exist.
            this.logger.LogError(_ =>
                "WARNING! By loading a different budget with a Transactions List Model loaded, data loss may occur. There may be budget buckets used in the transactions that do not exist in the new loaded Budget. This will result in those transactions being declassified. Check for unclassified transactions.");
        }
    }

    private async Task FinaliseSplitTransaction(ShellDialogResponseMessage message)
    {
        if (message.Response == ShellDialogButton.Save && SplitTransactionController.OriginalTransaction is not null)
        {
            this.transactionService.SplitTransaction(
                SplitTransactionController.OriginalTransaction,
                SplitTransactionController.SplinterAmount1,
                SplitTransactionController.SplinterAmount2,
                SplitTransactionController.SplinterBucket1!,
                SplitTransactionController.SplinterBucket2!); // Buckets validated by SplitTransactionController.

            FileOperations.NotifyOfEdit();
            await FileOperations.SyncWithServiceAsync();
        }
    }

    private async void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
    {
        // Budget ready message will always arrive before transactions are loaded from application state.
        if (!message.ActiveBudget.BudgetActive)
        {
            // Not the current budget for today so ignore this one.
            return;
        }

        await CheckBudgetContainsAllUsedBucketsFromTransactions(message.Budgets);
        ViewModel.TriggerRefreshBucketFilterList();
    }

    private void OnClosedNotificationReceived(object? sender, EventArgs e)
    {
        FileOperations.Close();
    }

    private void OnEditTransactionCommandExecute()
    {
        if (ViewModel.SelectedRow is null || this.shellDialogCorrelationId != Guid.Empty)
        {
            return;
        }

        EditingTransactionController.ShowDialog(ViewModel.SelectedRow);
    }

    private void OnGlobalDateFilterApplied(FilterAppliedMessage message)
    {
        if (message.Sender == this)
        {
            return;
        }

        if (ViewModel.TransactionsList is null)
        {
            return;
        }

        var bucketFilter = BucketFilter;
        BucketFilter = null;
        this.transactionService.FilterTransactions(message.Criteria);
        ViewModel.TransactionsList = this.transactionService.TransactionsListModel;
        ViewModel.Transactions = ViewModel.TransactionsList!.Transactions.ToList();
        CurrentPage = 1;
        ViewModel.TriggerRefreshTotalsRow();
        BucketFilter = bucketFilter;
        TextFilter = null;
    }

    private async void OnMergeExtractCommandExecute()
    {
        TextFilter = null;
        BucketFilter = null;
        CurrentPage = 1;
        await FileOperations.MergeInNewTransactions();
        UpdatePagedTransactions();
    }

    private async void OnNewDataSourceAvailableNotificationReceived(object? sender, EventArgs e)
    {
        CurrentPage = 1;
        await FileOperations.SyncWithServiceAsync();
        UpdatePagedTransactions();
    }

    private void OnSavedNotificationReceived(object? sender, EventArgs e)
    {
        FileOperations.ViewModel.Dirty = false;
    }

    private void OnShellDialogResponseMessageReceived(TopTransactionsListController recipient, ShellDialogResponseMessage message)
    {
        // TODO Could the Split Transaction Controller and Edit Transaction Controller's own thier own finalising and creation?
        if (!message.IsItForMe(this.shellDialogCorrelationId))
        {
            return;
        }

        if (message.Content is SplitTransactionController)
        {
            ObserveUnhandledFireAndForgetFailure(
                FinaliseSplitTransaction(message),
                ex => this.logger.LogError(ex, _ => "Unhandled exception processing SplitTransactionController in TopTransactionsListController."));
        }

        this.shellDialogCorrelationId = Guid.Empty;
    }

    private void OnSplitTransactionCommandExecute()
    {
        if (ViewModel.SelectedRow is null)
        {
            return;
        }

        this.shellDialogCorrelationId = Guid.NewGuid();
        SplitTransactionController.ShowDialog(ViewModel.SelectedRow, this.shellDialogCorrelationId);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TransactionsListViewModel.SelectedRow))
        {
            EditingTransactionController.Transaction = ViewModel.SelectedRow;
            UpdateCommandCanExecute();
        }
    }

    private void UpdateCommandCanExecute()
    {
        EditingTransactionController.DeleteTransactionCommand.NotifyCanExecuteChanged();
        EditTransactionCommand.NotifyCanExecuteChanged();
        SplitTransactionCommand.NotifyCanExecuteChanged();
        NavigateNextPageCommand.NotifyCanExecuteChanged();
        NavigatePreviousPageCommand.NotifyCanExecuteChanged();
    }

    private void UpdatePagedTransactions()
    {
        CanNavigateNext = CanNavigatePrevious = false;
        ViewModel.PagedTransactions = new ObservableCollection<Transaction>(ViewModel.Transactions.Skip((CurrentPage - 1) * PageSize).Take(PageSize));
        CanNavigateNext = CurrentPage < TotalPages;
        CanNavigatePrevious = CurrentPage > 1;
        UpdateCommandCanExecute();
    }
}
