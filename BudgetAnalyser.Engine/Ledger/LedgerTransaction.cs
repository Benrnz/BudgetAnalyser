using System;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
    //TODO Extract a specific subclass transaction type. This will allow removal of BankAccount from this base class.
    public abstract class LedgerTransaction
    {
        protected LedgerTransaction()
        {
            Id = Guid.NewGuid();
        }

        protected LedgerTransaction(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets the Bank Account for this transaction.  
        /// It represents which bank account the transaction applied to. This is particularly relevant for Balance Adjustment Transactions.
        /// In the case of <see cref="LedgerEntry"/> transactions it is set by the <see cref="LedgerEntry.LedgerColumn"/>.
        /// </summary>
        public AccountType BankAccount { get; internal set; }

        /// <summary>
        ///     Gets the amount of the credit value.
        ///     Both Credit and Debit cannot be set at the same time.
        ///     Values will be positive unless its a reversal.
        /// </summary>
        public decimal Credit { get; internal set; }

        /// <summary>
        ///     Gets the amount of the debit value.
        ///     Both Credit and Debit cannot be set at the same time.
        ///     Values will be positive unless its a reversal.
        /// </summary>
        public decimal Debit { get; internal set; }

        /// <summary>
        /// Gets or sets the Transaction ID. This is the same ID as the <see cref="StatementModel"/>'s <see cref="Transaction"/>.
        /// This can be used to link back to the statement and show more transaction specific data.
        /// </summary>
        public Guid Id { get; private set; }

        public string Narrative { get; internal set; }

        public LedgerTransaction WithAccountType([NotNull] AccountType accountType)
        {
            if (accountType == null)
            {
                throw new ArgumentNullException("accountType");
            }

            this.BankAccount = accountType;
            return this;
        }

        public virtual LedgerTransaction WithAmount(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount cannot be zero or less than zero", "amount");
            }

            return this;
        }

        public virtual LedgerTransaction WithNarrative([NotNull] string narrative)
        {
            if (narrative == null)
            {
                throw new ArgumentNullException("narrative");
            }

            Narrative = narrative;
            return this;
        }

        public virtual LedgerTransaction WithReversal(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount cannot be zero or less than zero", "amount");
            }

            return this;
        }
    }
}