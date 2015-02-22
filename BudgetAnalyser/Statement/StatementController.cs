using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class StatementController : ControllerBase, IShowableController, IInitializableController
    {
        public const string SortByBucketKey = "Bucket";
        public const string SortByDateKey = "Date";
        private readonly ITransactionManagerService transactionService;
        private readonly IUiContext uiContext;
        private string doNotUseBucketFilter;
        private bool doNotUseShown;
        private string doNotUseTextFilter;
        private bool initialised;
        private Guid shellDialogCorrelationId;

        public StatementController(
            [NotNull] IUiContext uiContext,
            [NotNull] StatementControllerFileOperations fileOperations,
            [NotNull] ITransactionManagerService transactionService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (fileOperations == null)
            {
                throw new ArgumentNullException("fileOperations");
            }

            if (transactionService == null)
            {
                throw new ArgumentNullException("transactionService");
            }

            FileOperations = fileOperations;
            this.uiContext = uiContext;
            this.transactionService = transactionService;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<FilterAppliedMessage>(this, OnGlobalDateFilterApplied);
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
            MessengerInstance.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseMessageReceived);

            this.transactionService.Closed += OnClosedNotificationReceived;
            this.transactionService.NewDatasourceAvailable += OnNewDatasourceAvailableNotificationReceived;
        }

        public AppliedRulesController AppliedRulesController
        {
            get { return this.uiContext.AppliedRulesController; }
        }

        /// <summary>
        ///     Gets or sets the bucket filter.
        ///     This is a string filter on the bucket code plus blank for all, and "[Uncatergorised]" for anything without a
        ///     bucket.
        ///     Only relevant when the view is displaying transactions by date.  The filter is hidden when shown in GroupByBucket
        ///     mode.
        /// </summary>
        public string BucketFilter
        {
            get { return this.doNotUseBucketFilter; }

            set
            {
                this.doNotUseBucketFilter = value;
                RaisePropertyChanged(() => BucketFilter);
                ViewModel.Transactions = this.transactionService.FilterByBucket(BucketFilter);
                ViewModel.TriggerRefreshTotalsRow();
            }
        }

        [Engine.Annotations.UsedImplicitly]
        public ICommand ClearTextFilterCommand
        {
            get
            {
                return new RelayCommand(
                    () =>
                    {
                        TextFilter = null;
                        ViewModel.Transactions = this.transactionService.ClearBucketAndTextFilters();
                    },
                    () => !String.IsNullOrWhiteSpace(TextFilter));
            }
        }

        public ICommand DeleteTransactionCommand
        {
            get { return new RelayCommand(OnDeleteTransactionCommandExecute, ViewModel.HasSelectedRow); }
        }

        internal EditingTransactionController EditingTransactionController
        {
            get { return this.uiContext.EditingTransactionController; }
        }

        public ICommand EditTransactionCommand
        {
            get { return new RelayCommand(OnEditTransactionCommandExecute, ViewModel.HasSelectedRow); }
        }

        public StatementControllerFileOperations FileOperations { get; private set; }

        [Engine.Annotations.UsedImplicitly]
        public ICommand MergeStatementCommand
        {
            get { return new RelayCommand(OnMergeStatementCommandExecute, FileOperations.CanExecuteCloseStatementCommand); }
        }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                if (value == this.doNotUseShown)
                {
                    return;
                }
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
            }
        }

        [Engine.Annotations.UsedImplicitly]
        public ICommand SortCommand
        {
            get { return new RelayCommand(OnSortCommandExecute, CanExecuteSortCommand); }
        }

        [Engine.Annotations.UsedImplicitly]
        public ICommand SplitTransactionCommand
        {
            get { return new RelayCommand(OnSplitTransactionCommandExecute, ViewModel.HasSelectedRow); }
        }

        internal SplitTransactionController SplitTransactionController
        {
            get { return this.uiContext.SplitTransactionController; }
        }

        public string TextFilter
        {
            get { return this.doNotUseTextFilter; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    this.doNotUseTextFilter = null;
                }
                else
                {
                    this.doNotUseTextFilter = value;
                }

                RaisePropertyChanged(() => TextFilter);
                ViewModel.Transactions = this.transactionService.FilterBySearchText(TextFilter);
                ViewModel.TriggerRefreshTotalsRow();
            }
        }

        public StatementViewModel ViewModel
        {
            get { return FileOperations.ViewModel; }
        }

        public void Initialize()
        {
            if (this.initialised)
            {
                return;
            }

            this.initialised = true;
            FileOperations.Initialise(this.transactionService);
        }

        public void RegisterListener<T>(object listener, Action<T> handler)
        {
            MessengerInstance.Register(listener, handler);
        }

        private bool CanExecuteSortCommand()
        {
            return ViewModel.Statement != null && ViewModel.Statement.Transactions.Any();
        }

        private async Task CheckBudgetContainsAllUsedBucketsFromStatement(BudgetCollection budgets = null)
        {
            if (!await this.transactionService.ValidateWithCurrentBudgetsAsync(budgets))
            {
                this.uiContext.UserPrompts.MessageBox.Show(
                    "WARNING! By loading a different budget with a Statement loaded, data loss may occur. There may be budget buckets used in the Statement that do not exist in the new loaded Budget. This will result in those Statement Transactions being declassified. \nCheck for unclassified transactions.",
                    "Data Loss Wanring!");
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

        private void FinaliseSplitTransaction(ShellDialogResponseMessage message)
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
            }
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            var statementMetadata = message.ElementOfType<StatementApplicationStateV1>();
            if (statementMetadata == null)
            {
                return;
            }

            this.transactionService.Initialise(statementMetadata);
            ViewModel.SortByBucket = statementMetadata.SortByBucket ?? false;
            if (ViewModel.Statement != null)
            {
                OnSortCommandExecute();
            }
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            StatementApplicationStateV1 statementMetadata = this.transactionService.PreparePersistentStateData();
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

        private void OnDeleteTransactionCommandExecute()
        {
            if (ViewModel.SelectedRow == null)
            {
                return;
            }

            bool? confirm = this.uiContext.UserPrompts.YesNoBox.Show(
                "Are you sure you want to delete this transaction?",
                "Delete Transaction");
            if (confirm != null && confirm.Value)
            {
                this.transactionService.RemoveTransaction(ViewModel.SelectedRow);
                ViewModel.TriggerRefreshTotalsRow();
                FileOperations.NotifyOfEdit();
            }
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
            RaisePropertyChanged(() => BucketFilter);
        }

        private async void OnMergeStatementCommandExecute()
        {
            TextFilter = null;
            BucketFilter = null;
            await FileOperations.MergeInNewTransactions();
        }

        private async void OnNewDatasourceAvailableNotificationReceived(object sender, EventArgs e)
        {
            await FileOperations.SyncWithServiceAsync();
        }

        private void OnShellDialogResponseMessageReceived(ShellDialogResponseMessage message)
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
                FinaliseSplitTransaction(message);
            }

            this.shellDialogCorrelationId = Guid.Empty;
        }

        private void OnSortCommandExecute()
        {
            BucketFilter = null;
            TextFilter = null;

            // The bindings are processed before commands, so the bound boolean for SortByBucket will be set to true by now.
            ViewModel.UpdateGroupedByBucket();
        }

        private void OnSplitTransactionCommandExecute()
        {
            this.shellDialogCorrelationId = Guid.NewGuid();
            SplitTransactionController.ShowDialog(ViewModel.SelectedRow, this.shellDialogCorrelationId);
        }
    }
}