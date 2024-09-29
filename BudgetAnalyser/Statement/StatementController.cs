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
using ApplicationStateLoadedMessage = BudgetAnalyser.ApplicationState.ApplicationStateLoadedMessage;
using ApplicationStateRequestedMessage = BudgetAnalyser.ApplicationState.ApplicationStateRequestedMessage;

namespace BudgetAnalyser.Statement;

[AutoRegisterWithIoC(SingleInstance = true)]
public class StatementController : ControllerBase, IShowableController, IInitializableController
{
    private readonly ITransactionManagerService transactionService;
    private readonly IUiContext uiContext;
    private string doNotUseBucketFilter;
    private bool doNotUseShown;
    private string? doNotUseTextFilter;
    private bool initialised;
    private Guid shellDialogCorrelationId;

    public StatementController(
        [NotNull] IUiContext uiContext,
        [NotNull] StatementControllerFileOperations fileOperations,
        [NotNull] ITransactionManagerService transactionService)
        : base(uiContext.Messenger)
    {
        FileOperations = fileOperations ?? throw new ArgumentNullException(nameof(fileOperations));
        this.uiContext = uiContext ?? throw new ArgumentNullException(nameof(uiContext));
        this.transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));

        Messenger.Register<StatementController, FilterAppliedMessage>(this, static (r, m) => r.OnGlobalDateFilterApplied(m));
        Messenger.Register<StatementController, ApplicationStateRequestedMessage>(this, static (r, m) => r.OnApplicationStateRequested(m));
        Messenger.Register<StatementController, ApplicationStateLoadedMessage>(this, static (r, m) => r.OnApplicationStateLoaded(m));
        Messenger.Register<StatementController, BudgetReadyMessage>(this, static (r, m) => r.OnBudgetReadyMessageReceived(m));
        Messenger.Register<StatementController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseMessageReceived(m));

        this.transactionService.Closed += OnClosedNotificationReceived;
        this.transactionService.NewDataSourceAvailable += OnNewDataSourceAvailableNotificationReceived;
        this.transactionService.Saved += OnSavedNotificationReceived;
    }

    public AppliedRulesController AppliedRulesController => this.uiContext.AppliedRulesController;

    /// <summary>
    ///     Gets or sets the bucket filter.
    ///     This is a string filter on the bucket code plus blank for all, and "[Uncatergorised]" for anything without a
    ///     bucket.
    ///     Only relevant when the view is displaying transactions by date.  The filter is hidden when shown in GroupByBucket
    ///     mode.
    /// </summary>
    public string BucketFilter
    {
        get => this.doNotUseBucketFilter;

        set
        {
            if (Equals(value, this.doNotUseBucketFilter)) return;
            this.doNotUseBucketFilter = value;
            OnPropertyChanged();
            ViewModel.Transactions = this.transactionService.FilterByBucket(BucketFilter);
            ViewModel.TriggerRefreshTotalsRow();
        }
    }

    public ICommand EditTransactionCommand => new RelayCommand(OnEditTransactionCommandExecute, ViewModel.HasSelectedRow);
    public StatementControllerFileOperations FileOperations { get; }

    [UsedImplicitly] public ICommand MergeStatementCommand => new RelayCommand(OnMergeStatementCommandExecute);

    [UsedImplicitly] public ICommand SplitTransactionCommand => new RelayCommand(OnSplitTransactionCommandExecute, ViewModel.HasSelectedRow);

    public string? TextFilter
    {
        get => this.doNotUseTextFilter;
        set
        {
            if (Equals(value, this.doNotUseTextFilter)) return;
            this.doNotUseTextFilter = string.IsNullOrEmpty(value) ? null : value;
            OnPropertyChanged();
            ViewModel.Transactions = this.transactionService.FilterBySearchText(TextFilter);
            ViewModel.TriggerRefreshTotalsRow();
        }
    }

    public StatementViewModel ViewModel => FileOperations.ViewModel;
    internal EditingTransactionController EditingTransactionController => this.uiContext.EditingTransactionController;
    internal SplitTransactionController SplitTransactionController => this.uiContext.SplitTransactionController;

    public void Initialize()
    {
        if (this.initialised)
        {
            return;
        }

        this.initialised = true;
        FileOperations.Initialise(this.transactionService);
    }

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

    public void RegisterListener<TMessage>(StatementUserControl recipient, MessageHandler<StatementUserControl, TMessage> handler) where TMessage : MessageBase
    {
        Messenger.Register(recipient, handler);
    }

    private async Task CheckBudgetContainsAllUsedBucketsFromStatement(BudgetCollection budgets = null)
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
        if (message.Response == ShellDialogButton.Save)
        {
            this.transactionService.SplitTransaction(
                SplitTransactionController.OriginalTransaction,
                SplitTransactionController.SplinterAmount1,
                SplitTransactionController.SplinterAmount2,
                SplitTransactionController.SplinterBucket1,
                SplitTransactionController.SplinterBucket2);

            ViewModel.TriggerRefreshTotalsRow();
            FileOperations.NotifyOfEdit();
            await FileOperations.SyncWithServiceAsync();
        }
    }

    private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
    {
        var statementMetadata = message.ElementOfType<StatementApplicationState>();
        if (statementMetadata == null)
        {
            return;
        }

        this.transactionService.Initialise(statementMetadata);
    }

    private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
    {
        var statementMetadata = this.transactionService.PreparePersistentStateData();
        message.PersistThisModel(statementMetadata);
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

    private void OnClosedNotificationReceived(object sender, EventArgs e)
    {
        FileOperations.Close();
    }

    private void OnEditTransactionCommandExecute()
    {
        if (ViewModel.SelectedRow == null || this.shellDialogCorrelationId != Guid.Empty)
        {
            return;
        }

        this.shellDialogCorrelationId = Guid.NewGuid();
        EditingTransactionController.ShowDialog(ViewModel.SelectedRow, this.shellDialogCorrelationId);
    }

    private void OnGlobalDateFilterApplied(FilterAppliedMessage message)
    {
        if (message.Sender == this || message.Criteria == null)
        {
            return;
        }

        if (ViewModel.Statement == null)
        {
            return;
        }

        this.transactionService.FilterTransactions(message.Criteria);
        ViewModel.Statement = this.transactionService.StatementModel;
        ViewModel.Transactions = new ObservableCollection<Transaction>(ViewModel.Statement.Transactions);
        ViewModel.TriggerRefreshTotalsRow();
        OnPropertyChanged(nameof(BucketFilter));
    }

    private async void OnMergeStatementCommandExecute()
    {
        TextFilter = null;
        BucketFilter = null;
        await FileOperations.MergeInNewTransactions();
    }

    private async void OnNewDataSourceAvailableNotificationReceived(object sender, EventArgs e)
    {
        await FileOperations.SyncWithServiceAsync();
    }

    private void OnSavedNotificationReceived(object sender, EventArgs e)
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
        this.shellDialogCorrelationId = Guid.NewGuid();
        SplitTransactionController.ShowDialog(ViewModel.SelectedRow, this.shellDialogCorrelationId);
    }
}