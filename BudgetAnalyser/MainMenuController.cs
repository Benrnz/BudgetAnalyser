using System;
using System.Windows.Input;
using BudgetAnalyser.Engine.Annotations;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser
{
    public class MainMenuController : ControllerBase, IInitializableController
    {
        private readonly UiContext uiContext;
        private bool doNotUseBudgetToggle;
        private bool doNotUseDashboardToggle;
        private bool doNotUseLedgerBookToggle;
        private bool doNotUseReportsToggle;
        private bool doNotUseTransactionsToggle;

        public MainMenuController([NotNull] UiContext uiContext)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            this.uiContext = uiContext;
        }

        public ICommand BudgetCommand
        {
            get { return new RelayCommand(OnBudgetExecuted); }
        }

        public bool BudgetToggle
        {
            get { return this.doNotUseBudgetToggle; }
            set
            {
                this.doNotUseBudgetToggle = value;
                RaisePropertyChanged(() => BudgetToggle);
            }
        }

        public ICommand DashboardCommand
        {
            get { return new RelayCommand(OnDashboardExecuted); }
        }

        public bool DashboardToggle
        {
            get { return this.doNotUseDashboardToggle; }
            set
            {
                this.doNotUseDashboardToggle = value;
                RaisePropertyChanged(() => DashboardToggle);
            }
        }

        public ICommand LedgerBookCommand
        {
            get { return new RelayCommand(OnLedgerBookExecuted); }
        }

        public bool LedgerBookToggle
        {
            get { return this.doNotUseLedgerBookToggle; }
            set
            {
                this.doNotUseLedgerBookToggle = value;
                RaisePropertyChanged(() => LedgerBookToggle);
            }
        }

        public ICommand ReportsCommand
        {
            get { return new RelayCommand(OnReportsExecuted); }
        }

        public bool ReportsToggle
        {
            get { return this.doNotUseReportsToggle; }
            set
            {
                this.doNotUseReportsToggle = value;
                RaisePropertyChanged(() => ReportsToggle);
            }
        }

        public ICommand TransactionsCommand
        {
            get { return new RelayCommand(OnTransactionExecuted); }
        }

        public bool TransactionsToggle
        {
            get { return this.doNotUseTransactionsToggle; }
            set
            {
                this.doNotUseTransactionsToggle = value;
                RaisePropertyChanged(() => TransactionsToggle);
            }
        }

        public void Initialize()
        {
            DashboardCommand.Execute(null);
        }

        private void AfterTabExecutedCommon()
        {
            this.uiContext.DashboardController.Shown = DashboardToggle;
            this.uiContext.StatementController.Shown = TransactionsToggle;
            this.uiContext.LedgerBookController.Shown = LedgerBookToggle;
            this.uiContext.BudgetController.Shown = BudgetToggle;
            this.uiContext.ReportsCatalogController.Shown = ReportsToggle;
        }

        private void BeforeTabExecutedCommon()
        {
            DashboardToggle = false;
            TransactionsToggle = false;
            LedgerBookToggle = false;
            BudgetToggle = false;
            ReportsToggle = false;
        }

        private void OnBudgetExecuted()
        {
            BeforeTabExecutedCommon();
            BudgetToggle = true;
            AfterTabExecutedCommon();
        }

        private void OnDashboardExecuted()
        {
            BeforeTabExecutedCommon();
            DashboardToggle = true;
            AfterTabExecutedCommon();
        }

        private void OnLedgerBookExecuted()
        {
            BeforeTabExecutedCommon();
            LedgerBookToggle = true;
            AfterTabExecutedCommon();
        }

        private void OnReportsExecuted()
        {
            BeforeTabExecutedCommon();
            ReportsToggle = true;
            AfterTabExecutedCommon();
        }

        private void OnTransactionExecuted()
        {
            BeforeTabExecutedCommon();
            TransactionsToggle = true;
            AfterTabExecutedCommon();
        }
    }
}