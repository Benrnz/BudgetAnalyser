using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A transaction type exclusively for Bank Balance adjustments.  This type has an account to identify which bank
    ///     account it applies to.
    /// </summary>
    public class BankBalanceAdjustmentTransaction : LedgerTransaction
    {
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
        public Account.Account BankAccount { get; internal set; }

        internal BankBalanceAdjustmentTransaction WithAccount([NotNull] Account.Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            BankAccount = account;
            return this;
        }
    }
}