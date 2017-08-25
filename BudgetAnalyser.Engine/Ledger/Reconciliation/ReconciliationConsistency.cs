using System;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    [AutoRegisterWithIoC]
    internal class ReconciliationConsistency : IReconciliationConsistency
    {
        public IDisposable EnsureConsistency(LedgerBook book)
        {
            return new ReconciliationConsistencyChecker(book);
        }
    }
}