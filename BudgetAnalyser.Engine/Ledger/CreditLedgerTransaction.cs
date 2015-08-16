using System;

namespace BudgetAnalyser.Engine.Ledger
{
    public class CreditLedgerTransaction : LedgerTransaction
    {
        /// <summary>
        ///     Creates a new instance of <see cref="CreditLedgerTransaction" />.
        ///     Called using reflection during deserialisation.
        /// </summary>
        public CreditLedgerTransaction(Guid id)
            : base(id)
        {
        }

        internal CreditLedgerTransaction()
        {
        }
    }
}