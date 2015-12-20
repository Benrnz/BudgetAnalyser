using System;

namespace BudgetAnalyser.Engine.Ledger
{
    internal interface IReconciliationConsistency
    {
        IDisposable EnsureConsistency(LedgerBook book);
    }
}