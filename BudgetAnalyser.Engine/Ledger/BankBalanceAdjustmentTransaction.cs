using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A transaction type exclusively for Bank Balance adjustments.  This type has an account to identify which bank
    ///     account it applies to.
    /// </summary>
    public class BankBalanceAdjustmentTransaction : LedgerTransaction
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BankBalanceAdjustmentTransaction" /> class.
        /// </summary>
        /// <param name="id"></param>
        public BankBalanceAdjustmentTransaction(Guid id) : base(id)
        {
        }

        internal BankBalanceAdjustmentTransaction()
        {
        }

        /// <summary>
        ///     Gets or sets the Bank Account for this transaction.
        ///     It represents which bank account the transaction applied to. This is particularly relevant for Balance Adjustment
        ///     Transactions.
        ///     In the case of <see cref="LedgerEntry" /> transactions it is set by the <see cref="LedgerEntry.LedgerBucket" />.
        /// </summary>
        public required Account BankAccount { get; init; }
    }
}
