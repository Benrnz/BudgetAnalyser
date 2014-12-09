using System;
using System.Linq;
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

        private readonly IUiContext uiContext;
        private readonly ITransactionManagerService transactionService;
        private bool doNotUseShown;
        private string doNotUseTextFilter;
        private bool filterByTextActive;
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
            MessengerInstance.Register<FilterAppliedMessage>(this, OnFilterApplied);
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
            MessengerInstance.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseMessageReceived);
        }

        public AppliedRulesController AppliedRulesController
        {
            get { return this.uiContext.AppliedRulesController; }
        }

        public ICommand ClearTextFilterCommand
        {
            get { return new RelayCommand(OnClearTextFilterCommandExecute, () => !string.IsNullOrWhiteSpace(TextFilter)); }
        }

        public ICommand DeleteTransactionCommand
        {
            get { return new RelayCommand(OnDeleteTransactionCommandExecute, ViewModel.HasSelectedRow); }
        }

        public ICommand EditTransactionCommand
        {
            get { return new RelayCommand(OnEditTransactionCommandExecute, ViewModel.HasSelectedRow); }
        }

        public StatementControllerFileOperations FileOperations { get; private set; }

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

        public ICommand SortCommand
        {
            get { return new RelayCommand(OnSortCommandExecute, CanExecuteSortCommand); }
        }

        public ICommand SplitTransactionCommand
        {
            get { return new RelayCommand(OnSplitTransactionCommandExecute, ViewModel.HasSelectedRow); }
        }

        public string TextFilter
        {
            get { return this.doNotUseTextFilter; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.doNotUseTextFilter = null;
                }
                else
                {
                    this.doNotUseTextFilter = value;
                }

                RaisePropertyChanged(() => TextFilter);
                PerformTextSearch(TextFilter);
            }
        }

        public StatementViewModel ViewModel
        {
            get { return FileOperations.ViewModel; }
        }

        internal EditingTransactionController EditingTransactionController
        {
            get { return this.uiContext.EditingTransactionController; }
        }

        internal SplitTransactionController SplitTransactionController
        {
            get { return this.uiContext.SplitTransactionController; }
        }

        public void Initialize()
        {
            if (this.initialised)
            {
                return;
            }

            this.initialised = true;
            FileOperations.Initialise(this);
            FileOperations.UpdateRecentFiles();
        }

        public void RegisterListener<T>(object listener, Action<T> handler)
        {
            MessengerInstance.Register(listener, handler);
        }

        private bool CanExecuteSortCommand()
        {
            return ViewModel.Statement != null && ViewModel.Statement.Transactions.Any();
        }

        private void ClearTextFilter()
        {
            var requestFilter = new RequestFilterMessage(this);
            MessengerInstance.Send(requestFilter);
            ViewModel.Statement.Filter(requestFilter.Criteria);
        }

        private void DeleteTransaction()
        {
            ViewModel.Statement.RemoveTransaction(ViewModel.SelectedRow);
            ViewModel.TriggerRefreshTotalsRow();
            FileOperations.NotifyOfEdit();
        }

        private void FinaliseEditTransaction(ShellDialogResponseMessage message)
        {
            if (message.Response == ShellDialogButton.Ok)
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
                ViewModel.Statement.SplitTransaction(
                    SplitTransactionController.OriginalTransaction,
                    SplitTransactionController.SplinterAmount1,
                    SplitTransactionController.SplinterAmount2,
                    SplitTransactionController.SplinterBucket1,
                    SplitTransactionController.SplinterBucket2);

                ViewModel.TriggerRefreshTotalsRow();
                FileOperations.NotifyOfEdit();
            }
        }

        private async void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (!message.RehydratedModels.ContainsKey(typeof(StatementApplicationStateV1)))
            {
                return;
            }

            var statementMetadata = ((StatementApplicationStateV1)message.RehydratedModels[typeof(StatementApplicationStateV1)]).StatementApplicationState;
            await FileOperations.LoadStatementFromApplicationStateAsync(statementMetadata.StorageKey);
            ViewModel.SortByBucket = statementMetadata.SortByBucket ?? false;
            OnSortCommandExecute();
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            var statementMetadata = new StatementApplicationStateV1
            {
                StatementApplicationState = new StatementApplicationState
                {
                    StorageKey = ViewModel.Statement == null ? null : ViewModel.Statement.StorageKey,
                    SortByBucket = ViewModel.SortByBucket,
                }
            };
            message.PersistThisModel(statementMetadata);
        }

        private async void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
        {
            if (!message.ActiveBudget.BudgetActive)
            {
                // Not the current budget for today so ignore.
                return;
            }

            BudgetModel oldBudget = ViewModel.BudgetModel;
            ViewModel.BudgetModel = message.ActiveBudget.Model;

            if (FileOperations.WaitingForBudgetToLoad != null)
            {
                // We've been waiting for the budget to load so we can load previous statement.
                await FileOperations.LoadStatementFromApplicationStateAsync(FileOperations.WaitingForBudgetToLoad);
                return;
            }

            if (oldBudget != null
                && (oldBudget.Expenses.Any() || oldBudget.Incomes.Any())
                && oldBudget.Name != ViewModel.BudgetModel.Name
                && ViewModel.Statement != null
                && ViewModel.Statement.AllTransactions.Any())
            {
                this.uiContext.UserPrompts.MessageBox.Show(
                    "WARNING! By loading a different budget with a Statement loaded, data loss may occur. There may be budget buckets used in the Statement that do not exist in the new loaded Budget. This will result in those Statement Transactions being declassified. \nCheck for unclassified transactions.",
                    "Data Loss Wanring!");
            }

            ViewModel.TriggerRefreshBucketFilterList();
        }

        private void OnClearTextFilterCommandExecute()
        {
            TextFilter = null;
            ClearTextFilter();
        }

        private void OnDeleteTransactionCommandExecute()
        {
            if (ViewModel.SelectedRow == null)
            {
                return;
            }

            bool? confirm = this.uiContext.UserPrompts.YesNoBox.Show(
                "Are you sure you want to delete this transaction?", "Delete Transaction");
            if (confirm != null && confirm.Value)
            {
                DeleteTransaction();
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

        private void OnFilterApplied(FilterAppliedMessage message)
        {
            if (message.Sender == this || message.Criteria == null)
            {
                return;
            }

            if (ViewModel.Statement == null)
            {
                return;
            }

            ViewModel.Statement.Filter(message.Criteria);
            ViewModel.TriggerRefreshTotalsRow();
            ViewModel.TriggerRefreshBucketFilter();
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
            // The bindings are processed before commands, so the bound boolean for SortByBucket will be set to true by now.
            ViewModel.UpdateGroupedByBucket();
        }

        private void OnSplitTransactionCommandExecute()
        {
            this.shellDialogCorrelationId = Guid.NewGuid();
            SplitTransactionController.ShowDialog(ViewModel.SelectedRow, this.shellDialogCorrelationId);
        }

        private void PerformTextSearch(string textFilter)
        {
            bool filtered = ViewModel.Statement.FilterByText(textFilter);

            if (string.IsNullOrWhiteSpace(TextFilter))
            {
                if (this.filterByTextActive && !filtered)
                {
                    ClearTextFilter();
                }

                this.filterByTextActive = false;
                return;
            }

            this.filterByTextActive = true;
        }
    }
}