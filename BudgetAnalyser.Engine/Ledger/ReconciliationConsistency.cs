using System;
using System.Linq;

namespace BudgetAnalyser.Engine.Ledger
{
    public interface IReconciliationConsistency
    {
        IDisposable EnsureConsistency(LedgerBook book);
    }

    [AutoRegisterWithIoC]
    public class ReconciliationConsistency : IReconciliationConsistency
    {
        public IDisposable EnsureConsistency(LedgerBook book)
        {
            return new ReconciliationConsistencyChecker { LedgerBook = book };
        }
    }

    public class ReconciliationConsistencyChecker : IDisposable
    {
        private readonly decimal check1;
        private decimal check2;

        public ReconciliationConsistencyChecker()
        {
            this.check1 = LedgerBook.Reconciliations.Sum(e => e.CalculatedSurplus);
        }

        public LedgerBook LedgerBook { get; set; }

        public void Dispose()
        {
            this.check2 = LedgerBook.Reconciliations.Sum(e => e.CalculatedSurplus) - LedgerBook.Reconciliations.First().CalculatedSurplus;
            if (this.check1 != this.check2)
            {
                throw new CorruptedLedgerBookException("Code Error: The previous dated entries have changed, this is not allowed. Data is corrupt.");
            }
        }
    }
}