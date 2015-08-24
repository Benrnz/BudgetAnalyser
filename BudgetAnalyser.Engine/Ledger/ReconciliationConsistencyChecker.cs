using System;
using System.Linq;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     This class is responsible for validating that changes were made during a reconciliation that did not alter the
    ///     Ledger Book in an inappropriate and invalid way.
    /// </summary>
    public class ReconciliationConsistencyChecker : IDisposable
    {
        private readonly decimal check1;

        private readonly LedgerBook ledgerBook;
        private decimal check2;

        public ReconciliationConsistencyChecker(LedgerBook book)
        {
            this.ledgerBook = book;
            this.check1 = this.ledgerBook.Reconciliations.Sum(e => e.CalculatedSurplus);
        }

        public void Dispose()
        {
            this.check2 = this.ledgerBook.Reconciliations.Sum(e => e.CalculatedSurplus) - this.ledgerBook.Reconciliations.First().CalculatedSurplus;
            if (this.check1 != this.check2)
            {
                throw new CorruptedLedgerBookException("Code Error: The previous dated entries have changed, this is not allowed. Data is corrupt.");
            }
        }
    }
}