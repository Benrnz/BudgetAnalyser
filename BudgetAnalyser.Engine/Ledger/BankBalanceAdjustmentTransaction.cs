using System;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    /// A transaction type exclusively for Bank Balance adjustments.  This type has an account type to identify which bank account it applies to.
    /// </summary>
    public class BankBalanceAdjustmentTransaction : LedgerTransaction
    {
        /// <summary>
        /// Gets or sets the Bank Account for this transaction.  
        /// It represents which bank account the transaction applied to. This is particularly relevant for Balance Adjustment Transactions.
        /// In the case of <see cref="LedgerEntry"/> transactions it is set by the <see cref="LedgerEntry.LedgerColumn"/>.
        /// </summary>
        public AccountType BankAccount { get; internal set; }

        public BankBalanceAdjustmentTransaction WithAccountType([NotNull] AccountType accountType)
        {
            if (accountType == null)
            {
                throw new ArgumentNullException("accountType");
            }

            BankAccount = accountType;
            return this;
        }
    }
}