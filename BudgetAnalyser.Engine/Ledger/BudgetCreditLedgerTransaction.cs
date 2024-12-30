using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A Ledger Transaction that represents a budgeted amount being credited to a ledger bucket.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Ledger.LedgerTransaction" />
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
