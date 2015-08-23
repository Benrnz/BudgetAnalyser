using System;

namespace BudgetAnalyser.Engine.Ledger
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