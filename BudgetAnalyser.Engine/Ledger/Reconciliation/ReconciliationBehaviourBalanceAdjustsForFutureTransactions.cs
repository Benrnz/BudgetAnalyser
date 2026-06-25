using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

[AutoRegisterWithIoC]
internal class ReconciliationBehaviourBalanceAdjustsForFutureTransactions : IReconciliationBehaviour
{
    public LedgerEntryLine? NewReconLine { get; private set; }

    public TransactionsListModel? Transactions { get; private set; }
    public IList<ToDoTask>? TodoTasks { get; private set; }

    public void Initialise(params object[] anyParameters)
    {
        foreach (var argument in anyParameters)
        {
            TodoTasks ??= argument as IList<ToDoTask>;
            NewReconLine ??= argument as LedgerEntryLine;
            Transactions ??= argument as TransactionsListModel;
        }

        if (TodoTasks is null)
        {
            throw new ArgumentNullException(nameof(TodoTasks));
        }

        if (NewReconLine is null)
        {
            throw new ArgumentNullException(nameof(NewReconLine));
        }

        if (Transactions is null)
        {
            throw new ArgumentNullException(nameof(Transactions));
        }
    }

    public void ApplyBehaviour()
    {
        if (Transactions is not null)
        {
            AddBalanceAdjustmentsForFutureTransactions(Transactions, NewReconLine!, TodoTasks!);
        }
    }

    private void AddBalanceAdjustmentsForFutureTransactions(TransactionsListModel transactionsList, LedgerEntryLine reconciliation, IList<ToDoTask> tasks)
    {
        var adjustmentsMade = false;
        foreach (var futureTransaction in transactionsList.AllTransactions
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
