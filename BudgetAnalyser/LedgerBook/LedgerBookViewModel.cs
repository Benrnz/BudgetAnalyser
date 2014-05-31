using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerBookViewModel : ViewModelBase
    {
        private BudgetCurrencyContext doNotUseCurrentBudget;
        private StatementModel doNotUseCurrentStatement;
        private Engine.Ledger.LedgerBook ledgerBook;

        public Engine.Ledger.LedgerBook LedgerBook
        {
            get { return this.ledgerBook; }

            set
            {
                this.ledgerBook = value;
                RaisePropertyChanged(() => LedgerBook);
                RaisePropertyChanged(() => NoLedgerBookLoaded);
            }
        }

        public bool NoBudgetLoaded
        {
            get { return CurrentBudget == null; }
        }

        public bool NoLedgerBookLoaded
        {
            get { return LedgerBook == null; }
        }

        public bool NoStatementLoaded
        {
            get { return CurrentStatement == null; }
        }

        /// <summary>
        ///     This variable is used to contain the newly added ledger line when doing a new reconciliation. When this is non-null
        ///     it also indicates the ledger row can be edited.
        /// </summary>
        internal LedgerEntryLine NewLedgerLine { get; set; }

        internal BudgetCurrencyContext CurrentBudget
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
    }
}