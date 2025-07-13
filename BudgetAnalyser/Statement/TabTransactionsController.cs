using System.Collections.ObjectModel;
using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Statement;

[AutoRegisterWithIoC(SingleInstance = true)]
public class TabTransactionsController : ControllerBase, IShowableController
{
    private readonly ITransactionManagerService transactionService;
    private readonly IUiContext uiContext;
    private string? doNotUseBucketFilter;
    private bool doNotUseCanNavigateNext;
    private bool doNotUseCanNavigatePrevious;
    private int doNotUseCurrentPage = 1;
    private int doNotUsePageSize = 10;
    private bool doNotUseShown;
    private string? doNotUseTextFilter;
    private Guid shellDialogCorrelationId;

    public TabTransactionsController(
        IUiContext uiContext,
        StatementControllerFileOperations fileOperations,
        ITransactionManagerService transactionService)
        : base(uiContext.Messenger)
    {
        FileOperations = fileOperations ?? throw new ArgumentNullException(nameof(fileOperations));
        this.uiContext = uiContext ?? throw new ArgumentNullException(nameof(uiContext));
        this.transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));

        Messenger.Register<TabTransactionsController, FilterAppliedMessage>(this, static (r, m) => r.OnGlobalDateFilterApplied(m));
        Messenger.Register<TabTransactionsController, BudgetReadyMessage>(this, static (r, m) => r.OnBudgetReadyMessageReceived(m));
        Messenger.Register<TabTransactionsController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseMessageReceived(m));

        this.transactionService.Closed += OnClosedNotificationReceived;
        this.transactionService.NewDataSourceAvailable += OnNewDataSourceAvailableNotificationReceived;
        this.transactionService.Saved += OnSavedNotificationReceived;
    }

    public AppliedRulesController AppliedRulesController => this.uiContext.Controller<AppliedRulesController>();

    /// <summary>
    ///     Gets or sets the bucket filter.
    ///     This is a string filter on the bucket code plus blank for all, and "[Uncatergorised]" for anything without a
    ///     bucket.
    ///     Only relevant when the view is displaying transactions by date.  The filter is hidden when shown in GroupByBucket
    ///     mode.
    /// </summary>
    public string? BucketFilter
    {
        get => this.doNotUseBucketFilter;

        set
        {
            if (Equals(value, this.doNotUseBucketFilter))
            {
                return;
            }

            this.doNotUseBucketFilter = value;
            OnPropertyChanged();
            ViewModel.Transactions = this.transactionService.FilterByBucket(BucketFilter);
            ViewModel.TriggerRefreshTotalsRow();
            CurrentPage = 1;
        }
    }

    public bool CanNavigateNext
    {
        get => this.doNotUseCanNavigateNext;
        private set
        {
            if (value == this.doNotUseCanNavigateNext)
            {
                return;
            }

            this.doNotUseCanNavigateNext = value;
            OnPropertyChanged();
        }
    }

    public bool CanNavigatePrevious
    {
        get => this.doNotUseCanNavigatePrevious;
        private set
        {
            if (value == this.doNotUseCanNavigatePrevious)
            {
                return;
            }

            this.doNotUseCanNavigatePrevious = value;
            OnPropertyChanged();
        }
    }

    public int CurrentPage
    {
        get => this.doNotUseCurrentPage;
        set
        {
            this.doNotUseCurrentPage = value;
            UpdatePagedTransactions();
            OnPropertyChanged();
        }
    }

    public ICommand DeleteTransactionCommand => new RelayCommand(OnDeleteTransactionCommandExecute, ViewModel.HasSelectedRow);
    internal EditingTransactionController EditingTransactionController => this.uiContext.Controller<EditingTransactionController>();
    public ICommand EditTransactionCommand => new RelayCommand(OnEditTransactionCommandExecute, ViewModel.HasSelectedRow);
    public StatementControllerFileOperations FileOperations { get; }

    public ICommand MergeStatementCommand => new RelayCommand(OnMergeStatementCommandExecute);

    public int PageSize
    {
        get => this.doNotUsePageSize;
        set
        {
            if (value == this.doNotUsePageSize)
            {
                return;
            }

            if (value < 1)
            {
                value = 10;
            }

            this.doNotUsePageSize = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalPages));
        }
    }

    public ICommand SplitTransactionCommand => new RelayCommand(OnSplitTransactionCommandExecute, ViewModel.HasSelectedRow);

    internal SplitTransactionController SplitTransactionController => this.uiContext.Controller<SplitTransactionController>();

    public string? TextFilter
    {
        get => this.doNotUseTextFilter;
        set
        {
            if (Equals(value, this.doNotUseTextFilter))
            {
                return;
            }

            this.doNotUseTextFilter = string.IsNullOrEmpty(value) ? null : value;
            OnPropertyChanged();
            ViewModel.Transactions = this.transactionService.FilterBySearchText(TextFilter);
            ViewModel.TriggerRefreshTotalsRow();
            CurrentPage = 1;
        }
    }

    public int TotalPages => (ViewModel.Transactions.Count + PageSize - 1) / PageSize;

    public StatementViewModel ViewModel => FileOperations.ViewModel;

    public bool Shown
    {
        get => this.doNotUseShown;
        set
        {
            if (value == this.doNotUseShown)
            {
                return;
            }

            this.doNotUseShown = value;
            OnPropertyChanged();
        }
    }

    public void ClearSearch()
    {
        TextFilter = null;
        ViewModel.Transactions = this.transactionService.ClearBucketAndTextFilters();
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

    public void RegisterListener<TMessage>(StatementUserControl recipient, MessageHandler<StatementUserControl, TMessage> handler) where TMessage : MessageBase
    {
        Messenger.Register(recipient, handler);
    }

    private async Task CheckBudgetContainsAllUsedBucketsFromStatement(BudgetCollection? budgets = null)
    {
        if (!await this.transactionService.ValidateWithCurrentBudgetsAsync(budgets))
        {
            this.uiContext.UserPrompts.MessageBox.Show(
                "WARNING! By loading a different budget with a Statement loaded, data loss may occur. There may be budget buckets used in the Statement that do not exist in the new loaded Budget. This will result in those Statement Transactions being declassified. \nCheck for unclassified transactions.",
                "Data Loss Warning!");
        }
    }

    private void FinaliseEditTransaction(ShellDialogResponseMessage message)
    {
        if (message.Response == ShellDialogButton.Save)
        {
            var viewModel = (EditingTransactionController)message.Content;
            if (viewModel.HasChanged)
            {
                FileOperations.NotifyOfEdit();
            }
        }
    }

    private async Task FinaliseSplitTransaction(ShellDialogResponseMessage message)
    {
        if (message.Response == ShellDialogButton.Save && SplitTransactionController.OriginalTransaction is not null)
        {
            if (SplitTransactionController.SplinterBucket1 is null || SplitTransactionController.SplinterBucket2 is null)
            {
                this.uiContext.UserPrompts.MessageBox.Show("Splinter buckets cannot be empty.", "Split Transaction Validation Error");
                return;
            }

            this.transactionService.SplitTransaction(
                SplitTransactionController.OriginalTransaction,
                SplitTransactionController.SplinterAmount1,
                SplitTransactionController.SplinterAmount2,
                SplitTransactionController.SplinterBucket1,
                SplitTransactionController.SplinterBucket2);

            FileOperations.NotifyOfEdit();
            await FileOperations.SyncWithServiceAsync();
        }
    }

    private async void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
    {
        // Budget ready message will always arrive before statement is loaded from application state.
        if (!message.ActiveBudget.BudgetActive)
        {
            // Not the current budget for today so ignore this one.
            return;
        }

        await CheckBudgetContainsAllUsedBucketsFromStatement(message.Budgets);
        ViewModel.TriggerRefreshBucketFilterList();
    }

    private void OnClosedNotificationReceived(object? sender, EventArgs e)
    {
        FileOperations.Close();
    }

    private async void OnDeleteTransactionCommandExecute()
    {
        if (ViewModel.SelectedRow is null)
        {
            return;
        }

        var confirm = this.uiContext.UserPrompts.YesNoBox.Show(
            "Are you sure you want to delete this transaction?",
            "Delete Transaction");
        if (confirm is not null && confirm.Value)
        {
            this.transactionService.RemoveTransaction(ViewModel.SelectedRow);
            FileOperations.NotifyOfEdit();
            await FileOperations.SyncWithServiceAsync();
        }
    }

    private void OnEditTransactionCommandExecute()
    {
        if (ViewModel.SelectedRow is null || this.shellDialogCorrelationId != Guid.Empty)
        {
            return;
        }

        this.shellDialogCorrelationId = Guid.NewGuid();
        EditingTransactionController.ShowDialog(ViewModel.SelectedRow, this.shellDialogCorrelationId);
    }

    private void OnGlobalDateFilterApplied(FilterAppliedMessage message)
    {
        if (message.Sender == this)
        {
            return;
        }

        if (ViewModel.Statement is null)
        {
            return;
        }

        var bucketFilter = BucketFilter;
        BucketFilter = null;
        this.transactionService.FilterTransactions(message.Criteria);
        ViewModel.Statement = this.transactionService.StatementModel;
        ViewModel.Transactions = ViewModel.Statement!.Transactions.ToList();
        CurrentPage = 1;
        ViewModel.TriggerRefreshTotalsRow();
        BucketFilter = bucketFilter;
        TextFilter = null;
    }

    private async void OnMergeStatementCommandExecute()
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

    private async void OnShellDialogResponseMessageReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.shellDialogCorrelationId))
        {
            return;
        }

        if (message.Content is EditingTransactionController)
        {
            FinaliseEditTransaction(message);
        }
        else if (message.Content is SplitTransactionController)
        {
            await FinaliseSplitTransaction(message);
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

    private void UpdatePagedTransactions()
    {
        CanNavigateNext = CanNavigatePrevious = false;
        ViewModel.PagedTransactions = new ObservableCollection<Transaction>(ViewModel.Transactions.Skip((CurrentPage - 1) * PageSize).Take(PageSize));
        CanNavigateNext = CurrentPage < TotalPages;
        CanNavigatePrevious = CurrentPage > 1;
    }
}
