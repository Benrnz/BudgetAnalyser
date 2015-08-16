using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    public class BudgetCreditLedgerTransaction : LedgerTransaction
    {
        /// <summary>
        ///     Creates a new instance of <see cref="BudgetCreditLedgerTransaction" />.
        ///     Called using reflection during deserialisation.
        /// </summary>
        [UsedImplicitly]
        public BudgetCreditLedgerTransaction(Guid id)
            : base(id)
        {
        }

        internal BudgetCreditLedgerTransaction()
        {
        }
    }
}