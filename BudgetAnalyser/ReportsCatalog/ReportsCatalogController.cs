using System;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog.BurnDownGraphs;
using BudgetAnalyser.ReportsCatalog.LongTermSpendingLineGraph;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ReportsCatalogController : ControllerBase, IShowableController
    {
        private readonly NewWindowViewLoader newWindowViewLoader;
        private readonly Func<IDisposable> waitCursorFactory;
        private BudgetCollection budgets;
        private Engine.Ledger.LedgerBook currentLedgerBook;
        private StatementModel currentStatementModel;
        private bool doNotUseShown;

        public ReportsCatalogController([NotNull] UiContext uiContext, [NotNull] NewWindowViewLoader newWindowViewLoader)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (newWindowViewLoader == null)
            {
                throw new ArgumentNullException("newWindowViewLoader");
            }

            this.newWindowViewLoader = newWindowViewLoader;
            this.waitCursorFactory = uiContext.WaitCursorFactory;
            BudgetPieController = uiContext.BudgetPieController;
            LongTermSpendingGraphController = uiContext.LongTermSpendingGraphController;
            CurrentMonthBurnDownGraphsController = uiContext.CurrentMonthBurnDownGraphsController;
            OverallPerformanceController = uiContext.OverallPerformanceController;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<StatementReadyMessage>(this, OnStatementReadyMessageReceived);
            MessengerInstance.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            MessengerInstance.Register<LedgerBookReadyMessage>(this, OnLedgerBookReadyMessageReceived);
        }

        public ICommand BudgetPieCommand
        {
            get { return new RelayCommand(OnBudgetPieCommandExecute, CanExecuteBudgetPieCommand); }
        }

        public BudgetPieController BudgetPieController { get; private set; }

        public CurrentMonthBurnDownGraphsController CurrentMonthBurnDownGraphsController { get; private set; }

        public ICommand LongTermSpendingGraphCommand
        {
            get { return new RelayCommand(OnLongTermSpendingGraphCommandExecute, () => this.currentStatementModel != null); }
        }

        public LongTermSpendingGraphController LongTermSpendingGraphController { get; private set; }

        public ICommand OverallBudgetPerformanceCommand
        {
            get { return new RelayCommand(OnOverallBudgetPerformanceCommandExecute, CanExecuteOverallBudgetPerformanceCommand); }
        }

        public OverallPerformanceController OverallPerformanceController { get; private set; }

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

        public ICommand SpendingTrendCommand
        {
            get { return new RelayCommand(OnSpendingTrendCommandExecute, CanExecuteOverallBudgetPerformanceCommand); }
        }

        private bool CanExecuteBudgetPieCommand()
        {
            return this.budgets != null && this.budgets.CurrentActiveBudget != null;
        }

        private bool CanExecuteOverallBudgetPerformanceCommand()
        {
            return this.currentStatementModel != null
                   && this.currentStatementModel.Transactions.Any()
                   && this.budgets != null
                   && this.budgets.CurrentActiveBudget != null;
        }

        private void OnBudgetPieCommandExecute()
        {
            using (this.waitCursorFactory())
            {
                BudgetPieController.Load(this.budgets.CurrentActiveBudget);
            }

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
                throw new ArgumentNullException("message");
            }

            this.currentLedgerBook = message.LedgerBook;
        }

        private void OnLongTermSpendingGraphCommandExecute()
        {
            using (this.waitCursorFactory())
            {
                LongTermSpendingGraphController.Load(this.currentStatementModel, RequestCurrentFilter());
            }

            this.newWindowViewLoader.MinHeight = this.newWindowViewLoader.Height = 600;
            this.newWindowViewLoader.MinWidth = this.newWindowViewLoader.Width = 600;
            this.newWindowViewLoader.Show(LongTermSpendingGraphController);
        }

        private void OnOverallBudgetPerformanceCommandExecute()
        {
            using (this.waitCursorFactory())
            {
                OverallPerformanceController.Load(this.currentStatementModel, this.budgets, RequestCurrentFilter());
            }

            this.newWindowViewLoader.MinHeight = this.newWindowViewLoader.Height = 650;
            this.newWindowViewLoader.MinWidth = this.newWindowViewLoader.Width = 740;
            this.newWindowViewLoader.Show(OverallPerformanceController);
        }

        private void OnSpendingTrendCommandExecute()
        {
            using (this.waitCursorFactory())
            {
                CurrentMonthBurnDownGraphsController.Load(this.currentStatementModel, this.budgets.CurrentActiveBudget, RequestCurrentFilter(), this.currentLedgerBook);
            }

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
            MessengerInstance.Send(currentFilterMessage);
            return currentFilterMessage.Criteria;
        }
    }
}