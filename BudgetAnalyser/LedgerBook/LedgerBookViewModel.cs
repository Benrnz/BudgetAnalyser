using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerBookViewModel : ViewModelBase
    {
        private IBudgetCurrencyContext doNotUseCurrentBudget;
        private StatementModel doNotUseCurrentStatement;
        private Engine.Ledger.LedgerBook ledgerBook;
        private LedgerEntryLine doNotUseNewLedgerLine;

        public Engine.Ledger.LedgerBook LedgerBook
        {
            get { return this.ledgerBook; }

            set
            {
                this.ledgerBook = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => NoLedgerBookLoaded);
            }
        }

        /// <summary>
        ///     Is only referring to finding a budget based on DateTime.Today!  Do not use for reconciliation purposes or validation. Delegate down to the engine. 
        /// </summary>
        public bool NoBudgetLoaded => CurrentBudget == null;
        public bool NoLedgerBookLoaded => LedgerBook == null;
        public bool NoStatementLoaded => CurrentStatement == null;

        /// <summary>
        ///     CurrentBudget is not used for reconciliation purposes, for recon purposes this needs to find the effective budget for the recon date, NOT the current budget.
        ///     CurrentBudget should only be used for UI purposes such as an indication of current budgeted amount for something etc. 
        /// </summary>
        internal IBudgetCurrencyContext CurrentBudget
        {
            get { return this.doNotUseCurrentBudget; }
            set
            {
                this.doNotUseCurrentBudget = value;
                RaisePropertyChanged(() => NoBudgetLoaded);
            }
        }

        internal StatementModel CurrentStatement
        {
            get { return this.doNotUseCurrentStatement; }

            set
            {
                this.doNotUseCurrentStatement = value;
                RaisePropertyChanged(() => NoStatementLoaded);
            }
        }

        /// <summary>
        ///     This variable is used to contain the newly added ledger line when doing a new reconciliation. When this is non-null it also indicates the ledger row can be edited.
        /// </summary>
        public LedgerEntryLine NewLedgerLine
        {
            get { return this.doNotUseNewLedgerLine; }
            set
            {
                this.doNotUseNewLedgerLine = value;
                RaisePropertyChanged();
            }
        }
    }
}