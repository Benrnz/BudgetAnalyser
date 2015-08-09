using System;

namespace BudgetAnalyser.Engine.Ledger
{
    public class BudgetCreditLedgerTransaction : LedgerTransaction
    {
        /// <summary>
        ///     Creates a new instance of <see cref="BudgetCreditLedgerTransaction" />.
        ///     Called using reflection during deserialisation.
        /// </summary>
        public BudgetCreditLedgerTransaction(Guid id)
            : base(id)
        {
        }

        internal BudgetCreditLedgerTransaction()
        {
        }
    }
}