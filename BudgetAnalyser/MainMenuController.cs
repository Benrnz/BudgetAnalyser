using System;
using System.Diagnostics;
using System.Windows.Input;
using BudgetAnalyser.Engine.Annotations;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser
{
    public class MainMenuController : ControllerBase
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

        private void OnBudgetExecuted()
        {
            TabExecutedCommon();
            this.uiContext.BudgetController.Shown = BudgetToggle;
        }

        private void OnDashboardExecuted()
        {
            TabExecutedCommon();
            this.uiContext.DashboardController.Shown = DashboardToggle;
        }

        private void OnLedgerBookExecuted()
        {
            TabExecutedCommon();
            this.uiContext.LedgerBookController.Shown = LedgerBookToggle;
        }

        private void OnReportsExecuted()
        {
            TabExecutedCommon();
            
        }

        private void OnTransactionExecuted()
        {
            TabExecutedCommon();
        }

        private void TabExecutedCommon()
        {
            if (!DashboardToggle && !TransactionsToggle && !LedgerBookToggle && !BudgetToggle && !ReportsToggle)
            {
                DashboardToggle = true;
            }

            Debug.WriteLine("Dashboard: " + DashboardToggle);
            Debug.WriteLine("Transactions: " + TransactionsToggle);
            Debug.WriteLine("LedgerBook: " + LedgerBookToggle);
            Debug.WriteLine("Budget: " + BudgetToggle);
            Debug.WriteLine("Reports: " + ReportsToggle);
        }
    }
}