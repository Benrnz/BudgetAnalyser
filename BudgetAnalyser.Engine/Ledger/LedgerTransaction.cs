using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
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
        ///     Gets the amount.
        ///     Credits are positive and debits are negative.
        /// </summary>
        public decimal Amount { get; internal set; }

        /// <summary>
        /// Gets or sets the Transaction ID. This is the same ID as the <see cref="StatementModel"/>'s <see cref="Transaction"/>.
        /// This can be used to link back to the statement and show more transaction specific data.
        /// </summary>
        public Guid Id { get; private set; }

        public string Narrative { get; internal set; }

        public virtual LedgerTransaction WithAmount(decimal amount)
        {
            Amount = amount;
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
    }
}