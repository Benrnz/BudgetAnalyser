using System;

namespace BudgetAnalyser.Engine.Ledger
{
    public class DebitLedgerTransaction : LedgerTransaction
    {
        public DebitLedgerTransaction() : base()
        {
        }

        public DebitLedgerTransaction(Guid id)
            : base(id)
        {
        }

        public override LedgerTransaction WithAmount(decimal amount)
        {
            base.WithAmount(amount);
            Debit = amount;
            Credit = 0;
            return this;
        }

        public override LedgerTransaction WithReversal(decimal amount)
        {
            base.WithReversal(amount);
            Debit = -amount;
            Credit = 0;
            return this;
        }
    }
}