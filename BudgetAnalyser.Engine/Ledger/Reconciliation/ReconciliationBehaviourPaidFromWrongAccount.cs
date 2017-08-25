using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    /// <summary>
    ///     This reconciliation behaviour is responsible for looking for payments from a different account where the funds are
    ///     stored.
    ///     Once found, it will create a note task for the user to do a bank transfer, it'll show this transfer in advance in
    ///     the bucket.
    ///     It'll also add balance adjustments to correct the bank balances.
    /// </summary>
    public class ReconciliationBehaviourPaidFromWrongAccount
    {
        private IEnumerable<Transaction> transactions;
        private LedgerEntryLine newReconLine;
        private IList<ToDoTask> todoTasks;

        /// <summary>
        ///     Apply the behaviour to the reconciliation objects.
        /// </summary>
        public void ApplyBehaviour()
        {
            CreateTasksToTransferFundsIfPaidFromDifferentAccount();
        }

        /// <summary>
        ///     Initialise the behaviour class with any required input.
        /// </summary>
        public void Initialise([NotNull] IEnumerable<Transaction> statementTransactions, [NotNull] LedgerEntryLine newReconciliationLine, [NotNull] IList<ToDoTask> todoList)
        {
            this.todoTasks = todoList ?? throw new ArgumentNullException(nameof(todoList));
            this.newReconLine = newReconciliationLine ?? throw new ArgumentNullException(nameof(newReconciliationLine));
            this.transactions = statementTransactions ?? throw new ArgumentNullException(nameof(statementTransactions));
        }

        private void CreateTasksToTransferFundsIfPaidFromDifferentAccount()
        {
            var syncRoot = new object();
            Dictionary<BudgetBucket, Account> ledgerBuckets = this.newReconLine.Entries.Select(e => e.LedgerBucket)
                .Distinct()
                .ToDictionary(l => l.BudgetBucket, l => l.StoredInAccount);

            List<Transaction> debitAccountTransactionsOnly = this.transactions.Where(t => t.Account.AccountType != AccountType.CreditCard).ToList();

            // Amount < 0: This is because we are only interested in looking for debit transactions against a different account. These transactions will need to be transfered from the stored-in account.
            var proposedTasks = new List<Tuple<Transaction, TransferTask>>();
            Parallel.ForEach(
                debitAccountTransactionsOnly.Where(t => t.Amount < 0).ToList(),
                t =>
                {
                    if (!ledgerBuckets.ContainsKey(t.BudgetBucket))
                        return;
                    var ledgerAccount = ledgerBuckets[t.BudgetBucket];
                    if (t.Account != ledgerAccount)
                    {
                        var reference = ReferenceNumberGenerator.IssueTransactionReferenceNumber();
                        lock (syncRoot)
                        {
                            proposedTasks.Add(
                                Tuple.Create(
                                    t,
                                    new TransferTask(
                                        $"A {t.BudgetBucket.Code} payment for {t.Amount:C} on the {t.Date:d} has been made from {t.Account}, but funds are stored in {ledgerAccount}. Use reference {reference}",
                                        true)
                                    {
                                        Amount = -t.Amount,
                                        SourceAccount = ledgerAccount,
                                        DestinationAccount = t.Account,
                                        BucketCode = t.BudgetBucket.Code,
                                        Reference = reference
                                    }));
                        }
                    }
                });
            // Now check to ensure the detected transactions themselves are not one side of a journal style transfer.
            foreach (Tuple<Transaction, TransferTask> tuple in proposedTasks)
            {
                var suspectedPaymentTransaction = tuple.Item1;
                var transferTask = tuple.Item2;
                var matchingTransferTransaction = debitAccountTransactionsOnly.FirstOrDefault(
                    t => t.Amount == -suspectedPaymentTransaction.Amount
                         && t.Date == suspectedPaymentTransaction.Date
                         && t.BudgetBucket == suspectedPaymentTransaction.BudgetBucket
                         && t.Account != suspectedPaymentTransaction.Account
                         && t.Reference1 == suspectedPaymentTransaction.Reference1);
                if (matchingTransferTransaction == null)
                {
                    // No matching transaction exists - therefore the transaction is a payment.
                    this.todoTasks.Add(transferTask);
                }
            }

            // TODO Create Ledger Txns to show transfering from one account to another
            // TODO Create Balance Adjustment to show Sav bal is reduced.
        }
    }
}