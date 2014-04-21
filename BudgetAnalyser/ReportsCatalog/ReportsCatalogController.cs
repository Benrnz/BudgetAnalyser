using System;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.BurnDownGraphs;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.OverallPerformance;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog
{
    public class ReportsCatalogController : ControllerBase, IShowableController
    {
        private readonly IBudgetAnalysisView analysisFactory;
        private readonly Func<IDisposable> waitCursorFactory;
        private BudgetCollection budgets;
        private Engine.Ledger.LedgerBook currentLedgerBook;
        private StatementModel currentStatementModel;
        private bool doNotUseShown;

        public ReportsCatalogController(UiContext uiContext)
        {
            this.waitCursorFactory = uiContext.WaitCursorFactory;
            CurrentMonthBurnDownGraphsController = uiContext.CurrentMonthBurnDownGraphsController;
            this.analysisFactory = uiContext.AnalysisFactory;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<StatementReadyMessage>(this, OnStatementReadyMessageReceived);
            MessengerInstance.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            MessengerInstance.Register<LedgerBookReadyMessage>(this, OnLedgerBookReadyMessageReceived);
        }

        public ICommand AnalyseStatementCommand
        {
            get { return new RelayCommand(OnAnalyseStatementCommandExecute, CanExecuteAnalyseStatementCommand); }
        }

        public CurrentMonthBurnDownGraphsController CurrentMonthBurnDownGraphsController { get; private set; }

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
            get { return new RelayCommand(OnSpendingTrendCommandExecute, CanExecuteAnalyseStatementCommand); }
        }

        private bool CanExecuteAnalyseStatementCommand()
        {
            return this.currentStatementModel != null
                   && this.currentStatementModel.Transactions.Any()
                   && this.budgets != null
                   && this.budgets.CurrentActiveBudget != null;
        }

        private void OnAnalyseStatementCommandExecute()
        {
            OverallPerformanceBudgetAnalysis analysis;
            using (this.waitCursorFactory())
            {
                analysis = this.analysisFactory.Analyse(this.currentStatementModel, this.budgets, RequestCurrentFilter());
            }

            this.analysisFactory.ShowDialog(analysis);
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

        private void OnSpendingTrendCommandExecute()
        {
            using (this.waitCursorFactory())
            {
                CurrentMonthBurnDownGraphsController.Load(this.currentStatementModel, this.budgets.CurrentActiveBudget, RequestCurrentFilter(), this.currentLedgerBook);
            }
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