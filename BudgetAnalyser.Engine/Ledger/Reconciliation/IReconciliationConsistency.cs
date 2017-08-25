using System;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    internal interface IReconciliationConsistency
    {
        IDisposable EnsureConsistency(LedgerBook book);
    }
}