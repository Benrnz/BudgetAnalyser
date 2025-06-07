using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

/// <summary>
///     This reconciliation behaviour is responsible for looking for payments from a different account where the funds are
///     stored. Once found, it will create a note task for the user to do a bank transfer, it'll show this transfer in
///     advance in the bucket. It'll also add balance adjustments to correct the bank balances.
/// </summary>
[AutoRegisterWithIoC]
internal class ReconciliationBehaviourPaidFromWrongAccount : IReconciliationBehaviour
{
    private const string LogPrefix = "Ledger Reconciliation - ";
    private Dictionary<BudgetBucket, Account> ledgerBuckets = new();
    private ILogger? logger;

    public LedgerEntryLine? NewReconLine { get; private set; }

    public IList<ToDoTask>? TodoTasks { get; private set; }

    public IEnumerable<Transaction>? Transactions { get; private set; }

    /// <inheritdoc />
    public void ApplyBehaviour()
    {
        var wrongAccountPayments = DiscoverWrongAccountPaymentTransactions(Transactions!);

        var reference = ReferenceNumberGenerator.IssueTransactionReferenceNumber();

        // Remind the user to make a bank transfer to rectify the situation.
        CreateUserTasks(wrongAccountPayments, reference, TodoTasks!);

        CreateLedgerTransactionsShowingTransfer(NewReconLine!, wrongAccountPayments, reference);
    }

    /// <inheritdoc />
    public void Initialise(params object[] anyArguments)
    {
        foreach (var argument in anyArguments)
        {
            TodoTasks = TodoTasks ?? argument as IList<ToDoTask>;
            NewReconLine = NewReconLine ?? argument as LedgerEntryLine;
            Transactions = Transactions ?? argument as IEnumerable<Transaction>;
            this.logger = this.logger ?? argument as ILogger;
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

        if (this.logger is null)
        {
            throw new ArgumentNullException(nameof(this.logger));
        }

        this.ledgerBuckets = NewReconLine.Entries.Select(e => e.LedgerBucket)
            .Distinct()
            .ToDictionary(l => l.BudgetBucket, l => l.StoredInAccount);
    }

    private void CreateLedgerTransactionsShowingTransfer(LedgerEntryLine reconciliation, IEnumerable<Transaction> wrongAccountPayments, string reference)
    {
        foreach (var transaction in wrongAccountPayments)
        {
            var journal1 = new CreditLedgerTransaction
            {
                Amount = transaction.Amount, // Amount is already negative/debit
                AutoMatchingReference = reference,
                Date = reconciliation.Date,
                Narrative = "Transfer to rectify payment made from wrong account."
            };
            var journal2 = new CreditLedgerTransaction
            {
                Amount = -transaction.Amount,
                AutoMatchingReference = reference,
                Date = reconciliation.Date,
                Narrative = "Transfer to rectify payment made from wrong account."
            };
            var ledger = reconciliation.Entries.Single(l => l.LedgerBucket.BudgetBucket == transaction.BudgetBucket);
            var replacementTransactions = ledger.Transactions.ToList();
            replacementTransactions.Add(journal1);
            replacementTransactions.Add(journal2);
            ledger.SetTransactionsForReconciliation(replacementTransactions);

            reconciliation.BalanceAdjustment(
                transaction.Amount,
                $"Decrease balance to show transfer to rectify {ledger.LedgerBucket.BudgetBucket.Code} payment made from wrong account.",
                ledger.LedgerBucket.StoredInAccount);
            reconciliation.BalanceAdjustment(
                -transaction.Amount,
                $"Increase balance to show transfer to rectify {ledger.LedgerBucket.BudgetBucket.Code} payment made from wrong account.",
                transaction.Account);
        }
    }

    private void CreateUserTasks(IEnumerable<Transaction> wrongAccountPayments, string reference, IList<ToDoTask> todoTasks)
    {
        foreach (var transaction in wrongAccountPayments)
        {
            var ledgerAccount = this.ledgerBuckets[transaction.BudgetBucket!];
            todoTasks.Add(
                new TransferTask
                {
                    CanDelete = true,
                    Description =
                        $"A {transaction.BudgetBucket!.Code} payment for {transaction.Amount:C} on the {transaction.Date:d} has been made from {transaction.Account}, but funds are stored in {ledgerAccount}. Use reference {reference}",
                    Amount = -transaction.Amount,
                    SourceAccount = ledgerAccount,
                    DestinationAccount = transaction.Account,
                    BucketCode = transaction.BudgetBucket.Code,
                    Reference = reference
                });
        }
    }

    private IList<Transaction> DiscoverWrongAccountPaymentTransactions(IEnumerable<Transaction> transactions)
    {
        var debitAccountTransactionsOnly = transactions.Where(t => t.Account.AccountType != AccountType.CreditCard).ToList();

        var wrongAccountPayments = new List<Transaction>();

        // Amount < 0: This is because we are only interested in looking for debit transactions against a different account. These transactions will need to be transferred from the stored-in account.
        var syncRoot = new Lock();
        Parallel.ForEach(
            debitAccountTransactionsOnly.Where(t => t.Amount < 0).ToList(),
            t =>
            {
                if (!this.ledgerBuckets.TryGetValue(t.BudgetBucket!, out var ledgerAccount))
                {
                    return;
                }

                if (t.Account != ledgerAccount)
                {
                    lock (syncRoot)
                    {
                        wrongAccountPayments.Add(t);
                    }
                }
            });
        // Now check to ensure the detected transactions themselves are not one side of a journal style transfer.
        foreach (var suspectedPaymentTransaction in wrongAccountPayments.ToList())
        {
            var matchingTransferTransaction = debitAccountTransactionsOnly.FirstOrDefault(
                t => t.Amount == -suspectedPaymentTransaction.Amount
                     && t.Date == suspectedPaymentTransaction.Date
                     && t.BudgetBucket == suspectedPaymentTransaction.BudgetBucket
                     && t.Account != suspectedPaymentTransaction.Account
                     && t.Reference1 == suspectedPaymentTransaction.Reference1);
            if (matchingTransferTransaction is null)
            {
                // No matching transaction exists - therefore the transaction is not a journal, it's a payment from the wrong account.
                this.logger?.LogInfo(l => l.Format("{0}Found a payment from an account other than where the {1} funds are stored. {2}",
                    LogPrefix,
                    suspectedPaymentTransaction.BudgetBucket!.Code,
                    suspectedPaymentTransaction.Id));
            }
            else
            {
                wrongAccountPayments.Remove(suspectedPaymentTransaction);
            }
        }

        return wrongAccountPayments;
    }
}
