using System;

namespace BudgetAnalyser.Engine.Ledger
{
    public interface IReconciliationConsistency
    {
        IDisposable EnsureConsistency(LedgerBook book);
    }
}