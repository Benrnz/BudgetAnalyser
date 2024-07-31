using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog.BurnDownGraphs;
using BudgetAnalyser.ReportsCatalog.LongTermSpendingLineGraph;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Input;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ReportsCatalogController : ControllerBase, IShowableController
    {
        private readonly NewWindowViewLoader newWindowViewLoader;
        private BudgetCollection budgets;
        private Engine.Ledger.LedgerBook currentLedgerBook;
        private StatementModel currentStatementModel;
        private bool doNotUseShown;

        public ReportsCatalogController([NotNull] UiContext uiContext, [NotNull] NewWindowViewLoader newWindowViewLoader)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (newWindowViewLoader == null)
            {
                throw new ArgumentNullException(nameof(newWindowViewLoader));
            }

            this.newWindowViewLoader = newWindowViewLoader;
            BudgetPieController = uiContext.BudgetPieController;
            LongTermSpendingGraphController = uiContext.LongTermSpendingGraphController;
            CurrentMonthBurnDownGraphsController = uiContext.CurrentMonthBurnDownGraphsController;
            OverallPerformanceController = uiContext.OverallPerformanceController;

            Messenger = uiContext.Messenger;
            Messenger.Register<StatementReadyMessage>(this, OnStatementReadyMessageReceived);
            Messenger.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            Messenger.Register<LedgerBookReadyMessage>(this, OnLedgerBookReadyMessageReceived);
        }

        [UsedImplicitly]
        public ICommand BudgetPieCommand => new RelayCommand(OnBudgetPieCommandExecute, CanExecuteBudgetPieCommand);

        public BudgetPieController BudgetPieController { get; }
        public CurrentMonthBurnDownGraphsController CurrentMonthBurnDownGraphsController { get; }

        [UsedImplicitly]
        public ICommand LongTermSpendingGraphCommand => new RelayCommand(OnLongTermSpendingGraphCommandExecute, () => this.currentStatementModel != null);

        public LongTermSpendingGraphController LongTermSpendingGraphController { get; }

        [UsedImplicitly]
        public ICommand OverallBudgetPerformanceCommand => new RelayCommand(OnOverallBudgetPerformanceCommandExecute, CanExecuteOverallBudgetPerformanceCommand);

        public OverallPerformanceController OverallPerformanceController { get; }

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
                OnPropertyChanged();
            }
        }

        [UsedImplicitly]
        public ICommand SpendingTrendCommand => new RelayCommand(OnSpendingTrendCommandExecute, CanExecuteOverallBudgetPerformanceCommand);

        private bool CanExecuteBudgetPieCommand()
        {
            return this.budgets?.CurrentActiveBudget != null;
        }

        private bool CanExecuteOverallBudgetPerformanceCommand()
        {
            return this.currentStatementModel != null
                   && this.currentStatementModel.Transactions.Any()
                   && this.budgets?.CurrentActiveBudget != null;
        }

        private void OnBudgetPieCommandExecute()
        {
            BudgetPieController.Load(this.budgets.CurrentActiveBudget);

            this.newWindowViewLoader.MinHeight = this.newWindowViewLoader.Height = 600;
            this.newWindowViewLoader.MinWidth = this.newWindowViewLoader.Width = 800;
            this.newWindowViewLoader.Show(BudgetPieController);
        }

        private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
        {
            this.budgets = message.Budgets;
        }

        private void OnLedgerBookReadyMessageReceived([NotNull] LedgerBookReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            this.currentLedgerBook = message.LedgerBook;
        }

        private void OnLongTermSpendingGraphCommandExecute()
        {
            LongTermSpendingGraphController.Load(this.currentStatementModel, RequestCurrentFilter());
            if (LongTermSpendingGraphController.Graph == null)
            {
                // Error creating report, message to user is handled by the LongTermSpendingController.
                return;
            }

            this.newWindowViewLoader.MinHeight = this.newWindowViewLoader.Height = 600;
            this.newWindowViewLoader.MinWidth = this.newWindowViewLoader.Width = 600;
            this.newWindowViewLoader.Show(LongTermSpendingGraphController);
        }

        private void OnOverallBudgetPerformanceCommandExecute()
        {
            OverallPerformanceController.Load(this.currentStatementModel, this.budgets, RequestCurrentFilter());

            this.newWindowViewLoader.MinHeight = this.newWindowViewLoader.Height = 650;
            this.newWindowViewLoader.MinWidth = this.newWindowViewLoader.Width = 740;
            this.newWindowViewLoader.Show(OverallPerformanceController);
        }

        private void OnSpendingTrendCommandExecute()
        {
            CurrentMonthBurnDownGraphsController.Load(this.currentStatementModel, this.budgets.CurrentActiveBudget, RequestCurrentFilter(), this.currentLedgerBook);

            this.newWindowViewLoader.MinHeight = this.newWindowViewLoader.Height = 600;
            this.newWindowViewLoader.MinWidth = this.newWindowViewLoader.Width = 600;
            this.newWindowViewLoader.Show(CurrentMonthBurnDownGraphsController);
        }

        private void OnStatementReadyMessageReceived(StatementReadyMessage message)
        {
            this.currentStatementModel = message.StatementModel;
        }

        private GlobalFilterCriteria RequestCurrentFilter()
        {
            var currentFilterMessage = new RequestFilterMessage(this);
            Messenger.Send(currentFilterMessage);
            return currentFilterMessage.Criteria;
        }
    }
}