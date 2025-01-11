namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

/// <summary>
///     An overdrawn surplus balance is not valid, and indicates that one or more ledger buckets have been overdrawn.  A
///     transfer probably needs to be manually done by the user.
/// </summary>
[AutoRegisterWithIoC]
internal class ReconciliationBehaviourOverdrawnSurplus : IReconciliationBehaviour
{
    public LedgerEntryLine? NewReconLine { get; private set; }

    public IList<ToDoTask>? TodoTasks { get; private set; }

    public void Initialise(params object[] anyParameters)
    {
        foreach (var argument in anyParameters)
        {
            TodoTasks = TodoTasks ?? argument as IList<ToDoTask>;
            NewReconLine = NewReconLine ?? argument as LedgerEntryLine;
        }

        if (TodoTasks is null)
        {
            throw new ArgumentNullException(nameof(TodoTasks));
        }

        if (NewReconLine is null)
        {
            throw new ArgumentNullException(nameof(NewReconLine));
        }
    }

    public void ApplyBehaviour()
    {
        CreateToDoForAnyOverdrawnSurplusBalance(NewReconLine!, TodoTasks!);
    }

    private void CreateToDoForAnyOverdrawnSurplusBalance(LedgerEntryLine reconciliations, IList<ToDoTask> tasks)
    {
        foreach (var surplusBalance in reconciliations.SurplusBalances.Where(s => s.Balance < 0))
        {
            tasks.Add(
                new ToDoTask
                {
                    Description = $"{surplusBalance.Account} has a negative surplus balance {surplusBalance.Balance}, there must be one or more transfers to action.",
                    SystemGenerated = true,
                    CanDelete = true
                }
            );
        }
    }
}
