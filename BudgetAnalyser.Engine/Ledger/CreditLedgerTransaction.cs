using System;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A Ledger transactions that represents a credit value with a positive number and debit values with a negative number.
    ///     This is a general purpose transaction used for all transactions in a ledger except for crediting the period budget amount.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Ledger.LedgerTransaction" />
    public class CreditLedgerTransaction : LedgerTransaction
    {
        /// <summary>
        ///     Creates a new instance of <see cref="CreditLedgerTransaction" />.Called using reflection during deserialisation.
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
