using System;

namespace BudgetAnalyser.Engine.Ledger
{
    public class BudgetCreditLedgerTransaction : LedgerTransaction
    {
        internal BudgetCreditLedgerTransaction() : base() {  }

        /// <summary>
        /// Creates a new instance of <see cref="BudgetCreditLedgerTransaction"/>.
        /// Called using reflection during deserialisation.
        /// </summary>
        public BudgetCreditLedgerTransaction(Guid id)
            : base(id)
        {
        }
    }
}