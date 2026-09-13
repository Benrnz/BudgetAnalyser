namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

[AutoRegisterWithIoC]
internal class ReconciliationBehaviourBudgetAmountBalanceAdjustments : IReconciliationBehaviour
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
        CreateBalanceAdjustmentForBudgetAmounts(NewReconLine!, TodoTasks!);
    }

    private void CreateBalanceAdjustmentForBudgetAmounts(LedgerEntryLine reconciliation, IList<ToDoTask> todoTasks)
    {
        var transferTasks = todoTasks.OfType<TransferTask>().ToList();
        foreach (var grouping in transferTasks.GroupBy(t => t.SourceAccount, tasks => tasks))
        // Rather than create a task, just do it
        {
            reconciliation.BalanceAdjustment(
                -grouping.Sum(t => t.Amount),
                "Adjustment for moving budgeted amounts from income account. ",
                grouping.Key);
        }

        foreach (var grouping in transferTasks.GroupBy(t => t.DestinationAccount, tasks => tasks))
        // Rather than create a task, just do it
        {
            reconciliation.BalanceAdjustment(
                grouping.Sum(t => t.Amount),
                "Adjustment for moving budgeted amounts to destination account. ",
                grouping.Key);
        }
    }
}
