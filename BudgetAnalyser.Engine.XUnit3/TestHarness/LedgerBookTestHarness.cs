using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

public class LedgerBookTestHarness : LedgerBook
{
    /// <summary>
    ///     Constructs a new instance of the <see cref="LedgerBook" /> class.  The Persistence system calls this constructor,
    ///     not the IoC system.
    /// </summary>
    public LedgerBookTestHarness()
    {
    }

    public LedgerBookTestHarness(IEnumerable<LedgerEntryLine> reconciliations) : base(reconciliations)
    {
    }

    public Action<ReconciliationResult> ReconcileOverride { get; set; } = _ => { };

    internal override void Reconcile(ReconciliationResult newRecon)
    {
        ReconcileOverride.Invoke(newRecon);
    }
}
