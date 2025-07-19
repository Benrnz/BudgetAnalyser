using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

[AutoRegisterWithIoC]
internal class ReconciliationBehaviourBalanceAdjustsForFutureTransactions : IReconciliationBehaviour
{
    public LedgerEntryLine? NewReconLine { get; private set; }
    public IList<ToDoTask>? TodoTasks { get; private set; }

    public TransactionSetModel? TransactionSet { get; private set; }

    public void Initialise(params object[] anyParameters)
    {
        foreach (var argument in anyParameters)
        {
            TodoTasks ??= argument as IList<ToDoTask>;
            NewReconLine ??= argument as LedgerEntryLine;
            TransactionSet ??= argument as TransactionSetModel;
        }

        if (TodoTasks is null)
        {
            throw new ArgumentNullException(nameof(TodoTasks));
        }

        if (NewReconLine is null)
        {
            throw new ArgumentNullException(nameof(NewReconLine));
        }

        if (TransactionSet is null)
        {
            throw new ArgumentNullException(nameof(TransactionSet));
        }
    }

    public void ApplyBehaviour()
    {
        if (TransactionSet is not null)
        {
            AddBalanceAdjustmentsForFutureTransactions(TransactionSet, NewReconLine!, TodoTasks!);
        }
    }

    private void AddBalanceAdjustmentsForFutureTransactions(TransactionSetModel transactionSet, LedgerEntryLine reconciliation, IList<ToDoTask> tasks)
    {
        var adjustmentsMade = false;
        foreach (var futureTransaction in transactionSet.AllTransactions
                     .Where(t => t.Account.AccountType != AccountType.CreditCard
                                 && t.Date >= reconciliation.Date
                                 && !(t.BudgetBucket is PayCreditCardBucket)))
        {
            adjustmentsMade = true;
            reconciliation.BalanceAdjustment(-futureTransaction.Amount,
                $"Remove future transaction for {futureTransaction.Date:d} {futureTransaction.Description}",
                futureTransaction.Account);
        }

        if (adjustmentsMade)
        {
            tasks.Add(new ToDoTask { Description = "Check auto-generated balance adjustments for future transactions.", CanDelete = true, SystemGenerated = true });
        }
    }
}
