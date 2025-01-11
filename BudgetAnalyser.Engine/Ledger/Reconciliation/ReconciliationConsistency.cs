namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

/// <summary>
///     This class is used as a factory to create a <see cref="IDisposable"/> instance used to ensure the <see cref="LedgerBook"/> is left in a valid and consistent state after a reconciliation.
///     Even if the reconciliation fails and throws an exception, this mechanism ensures clean up code is run, effectively rolling back. As a factory class this gives a good unit testing
///     mechanism to exploit also.
/// </summary>
[AutoRegisterWithIoC]
internal class ReconciliationConsistency : IReconciliationConsistency
{
    public IDisposable EnsureConsistency(LedgerBook book)
    {
        return new ReconciliationConsistencyChecker(book);
    }
}
