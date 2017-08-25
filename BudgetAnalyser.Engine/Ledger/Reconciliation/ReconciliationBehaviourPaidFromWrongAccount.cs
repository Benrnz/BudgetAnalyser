using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    /// <summary>
    ///     This reconciliation behaviour is responsible for looking for payments from a different account where the funds are
    ///     stored. Once found, it will create a note task for the user to do a bank transfer, it'll show this transfer in
    ///     advance in the bucket. It'll also add balance adjustments to correct the bank balances.
    /// </summary>
    [AutoRegisterWithIoC]
    public class ReconciliationBehaviourPaidFromWrongAccount : IReconciliationBehaviour
    {
        private const string LogPrefix = "Ledger Reconciliation - ";
        private Dictionary<BudgetBucket, Account> ledgerBuckets;
        private ILogger logger;
        private LedgerEntryLine newReconLine;
        private IList<ToDoTask> todoTasks;
        private IEnumerable<Transaction> transactions;

        /// <inheritdoc />
        public void ApplyBehaviour()
        {
            IList<Transaction> wrongAccountPayments = DiscoverWrongAccountPaymentTransactions();

            var reference = ReferenceNumberGenerator.IssueTransactionReferenceNumber();

            // Remind the user to make a bank transfer to rectify the situation.
            CreateUserTasks(wrongAccountPayments, reference);

            CreateLedgerTransactionsShowingTransfer(wrongAccountPayments, reference);
        }

        /// <inheritdoc />
        public void Initialise(params KeyValuePair<string, object>[] anyArguments)
        {
            foreach (KeyValuePair<string, object> argument in anyArguments)
            {
                this.todoTasks = this.todoTasks ?? argument.Value as IList<ToDoTask>;
                this.newReconLine = this.newReconLine ?? argument.Value as LedgerEntryLine;
                this.transactions = this.transactions ?? argument.Value as IEnumerable<Transaction>;
                this.logger = this.logger ?? argument.Value as ILogger;
            }

            if (this.todoTasks == null)
            {
                throw new ArgumentNullException(nameof(this.todoTasks));
            }
            if (this.newReconLine == null)
            {
                throw new ArgumentNullException(nameof(this.newReconLine));
            }
            if (this.transactions == null)
            {
                throw new ArgumentNullException(nameof(this.transactions));
            }
            if (this.logger == null)
            {
                throw new ArgumentNullException(nameof(this.logger));
            }

            this.ledgerBuckets = this.newReconLine.Entries.Select(e => e.LedgerBucket)
                .Distinct()
                .ToDictionary(l => l.BudgetBucket, l => l.StoredInAccount);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.newReconLine = null;
            this.todoTasks = null;
            this.transactions = null;
            this.ledgerBuckets = null;
        }

        private void CreateLedgerTransactionsShowingTransfer(IEnumerable<Transaction> wrongAccountPayments, string reference)
        {
            foreach (var transaction in wrongAccountPayments)
            {
                var journal1 = new CreditLedgerTransaction
                {
                    Amount = transaction.Amount, // Amount is already negative/debit
                    AutoMatchingReference = reference,
                    Date = this.newReconLine.Date, // TODO check this
                    Narrative = "Transfer to rectify payment made from wrong account."
                };
                var journal2 = new CreditLedgerTransaction
                {
                    Amount = -transaction.Amount,
                    AutoMatchingReference = reference,
                    Date = this.newReconLine.Date,
                    Narrative = "Transfer to rectify payment made from wrong account."
                };
                var ledger = this.newReconLine.Entries.Single(l => l.LedgerBucket.BudgetBucket == transaction.BudgetBucket);
                ledger.AddTransaction(journal1);
                ledger.AddTransaction(journal2);

                this.newReconLine.BalanceAdjustment(transaction.Amount, $"Decrease balance to show transfer to rectify {ledger.LedgerBucket.BudgetBucket.Code} payment made from wrong account.",
                                                    ledger.LedgerBucket.StoredInAccount);
            }
        }

        private void CreateUserTasks(IEnumerable<Transaction> wrongAccountPayments, string reference)
        {
            foreach (var transaction in wrongAccountPayments)
            {
                var ledgerAccount = this.ledgerBuckets[transaction.BudgetBucket];
                this.todoTasks.Add(
                                   new TransferTask(
                                                    $"A {transaction.BudgetBucket.Code} payment for {transaction.Amount:C} on the {transaction.Date:d} has been made from {transaction.Account}, but funds are stored in {ledgerAccount}. Use reference {reference}",
                                                    true)
                                   {
                                       Amount = -transaction.Amount,
                                       SourceAccount = ledgerAccount,
                                       DestinationAccount = transaction.Account,
                                       BucketCode = transaction.BudgetBucket.Code,
                                       Reference = reference
                                   });
            }
        }

        private IList<Transaction> DiscoverWrongAccountPaymentTransactions()
        {
            List<Transaction> debitAccountTransactionsOnly = this.transactions.Where(t => t.Account.AccountType != AccountType.CreditCard).ToList();

            var wrongAccountPayments = new List<Transaction>();

            // Amount < 0: This is because we are only interested in looking for debit transactions against a different account. These transactions will need to be transfered from the stored-in account.
            var syncRoot = new object();
            Parallel.ForEach(
                             debitAccountTransactionsOnly.Where(t => t.Amount < 0).ToList(),
                             t =>
                             {
                                 if (!this.ledgerBuckets.ContainsKey(t.BudgetBucket))
                                 {
                                     return;
                                 }
                                 var ledgerAccount = this.ledgerBuckets[t.BudgetBucket];
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
                if (matchingTransferTransaction == null)
                {
                    // No matching transaction exists - therefore the transaction is not a journal, its a payment from the wrong account.
                    this.logger.LogInfo(l => l.Format("{0}Found a payment from an account other than where the {1} funds are stored. {2}",
                                                      LogPrefix,
                                                      suspectedPaymentTransaction.BudgetBucket.Code,
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
}