namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

[AutoRegisterWithIoC]
internal class ReconciliationConsistency : IReconciliationConsistency
{
    // TODO refactor this. Its silly to have this passing thru to another class.
    public IDisposable EnsureConsistency(LedgerBook book)
    {
        return new ReconciliationConsistencyChecker(book);
    }
}
