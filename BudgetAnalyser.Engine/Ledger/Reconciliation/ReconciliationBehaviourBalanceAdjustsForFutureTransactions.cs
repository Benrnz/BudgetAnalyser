using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

[AutoRegisterWithIoC]
internal class ReconciliationBehaviourBalanceAdjustsForFutureTransactions : IReconciliationBehaviour
{
    public LedgerEntryLine NewReconLine { get; private set; }

    public StatementModel Statement { get; private set; }
    public IList<ToDoTask> TodoTasks { get; private set; }

    public void Initialise(params object[] anyParameters)
    {
        foreach (var argument in anyParameters)
        {
            TodoTasks = TodoTasks ?? argument as IList<ToDoTask>;
            NewReconLine = NewReconLine ?? argument as LedgerEntryLine;
            Statement = Statement ?? argument as StatementModel;
        }

        if (TodoTasks is null)
        {
            throw new ArgumentNullException(nameof(TodoTasks));
        }

        if (NewReconLine is null)
        {
            throw new ArgumentNullException(nameof(NewReconLine));
        }

        if (Statement is null)
        {
            throw new ArgumentNullException(nameof(Statement));
        }
    }

    public void ApplyBehaviour()
    {
        if (Statement is not null)
        {
            AddBalanceAdjustmentsForFutureTransactions(Statement, NewReconLine.Date);
        }
    }

    private void AddBalanceAdjustmentsForFutureTransactions(StatementModel statement, DateTime reconciliationDate)
    {
        var adjustmentsMade = false;
        foreach (var futureTransaction in statement.AllTransactions
                     .Where(
                         t =>
                             t.Account.AccountType != AccountType.CreditCard && t.Date >= reconciliationDate &&
                             !(t.BudgetBucket is PayCreditCardBucket)))
        {
            adjustmentsMade = true;
            NewReconLine.BalanceAdjustment(-futureTransaction.Amount,
                $"Remove future transaction for {futureTransaction.Date:d} {futureTransaction.Description}",
                futureTransaction.Account);
        }

        if (adjustmentsMade)
        {
            TodoTasks.Add(new ToDoTask { Description = "Check auto-generated balance adjustments for future transactions.", CanDelete = true, SystemGenerated = true });
        }
    }
}
