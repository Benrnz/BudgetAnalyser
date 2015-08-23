using System;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC]
    public class ReconciliationConsistency : IReconciliationConsistency
    {
        public IDisposable EnsureConsistency(LedgerBook book)
        {
            return new ReconciliationConsistencyChecker { LedgerBook = book };
        }
    }
}