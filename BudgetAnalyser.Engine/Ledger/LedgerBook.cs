using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
    public class LedgerBook : IModelValidate
    {
        private readonly IReconciliationBuilder reconciliationBuilder;
        private readonly ILogger logger;
        private List<LedgerBucket> ledgersColumns = new List<LedgerBucket>();
        private List<LedgerEntryLine> reconciliations;

        /// <summary>
        /// Constructs a new instance of the <see cref="LedgerBook"/> class.  The Persistence system calls this constructor, not the IoC system. 
        /// </summary>
        public LedgerBook(ILogger logger, [NotNull] IReconciliationBuilder reconciliationBuilder)
        {
            if (reconciliationBuilder == null)
            {
                throw new ArgumentNullException(nameof(reconciliationBuilder));
            }
            this.reconciliationBuilder = reconciliationBuilder;
            this.reconciliationBuilder.LedgerBook = this;
            this.logger = logger ?? new NullLogger();
            this.reconciliations = new List<LedgerEntryLine>();
        }

        public string FileName { get; internal set; }

        /// <summary>
        ///     A mapping of Budget Buckets to Bank Accounts used to create the next instances of the <see cref="LedgerEntry" />
        ///     class
        ///     during the next reconciliation. Changing these values will only effect the next reconciliation, not current data.
        /// </summary>
        public IEnumerable<LedgerBucket> Ledgers
        {
            get { return this.ledgersColumns; }
            internal set { this.ledgersColumns = value.OrderBy(c => c.BudgetBucket.Code).ToList(); }
        }

        public DateTime Modified { get; internal set; }
        public string Name { get; internal set; }

        public IEnumerable<LedgerEntryLine> Reconciliations
        {
            get { return this.reconciliations; }
            [UsedImplicitly] private set { this.reconciliations = value.ToList(); }
        }

        public bool Validate([NotNull] StringBuilder validationMessages)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException(nameof(validationMessages));
            }

            if (string.IsNullOrWhiteSpace(FileName))
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "A ledger book must have a file name.");
                return false;
            }

            LedgerEntryLine line = Reconciliations.FirstOrDefault();
            if (line != null)
            {
                LedgerEntryLine previous = Reconciliations.Skip(1).FirstOrDefault();
                if (previous != null && line.Date <= previous.Date)
                {
                    validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Duplicate and or out of sequence dates exist in the reconciliations for this Ledger Book.");
                    return false;
                }

                if (!line.Validate(validationMessages, previous))
                {
                    return false;
                }
            }

            return true;
        }

        internal LedgerBucket AddLedger(ExpenseBucket budgetBucket, Account.Account storeInThisAccount)
        {
            if (this.ledgersColumns.Any(l => l.BudgetBucket == budgetBucket))
            {
                // Ledger already exists in this ledger book.
                return null;
            }

            var newLedger = new LedgerBucket { BudgetBucket = budgetBucket, StoredInAccount = storeInThisAccount };
            this.ledgersColumns.Add(newLedger);
            return newLedger;
        }

        /// <summary>
        ///     Creates a new LedgerEntryLine for this <see cref="LedgerBook" />.
        /// </summary>
        /// <param name="reconciliationStartDate">
        ///     The startDate for the <see cref="LedgerEntryLine" />. This is usually the previous Month's "Reconciliation-Date", as this month's reconciliation starts with this date and includes transactions
        ///     from that date. This date is different to the "Reconciliation-Date" that appears next to the resulting reconciliation which is the end date for the period.
        /// </param>
        /// <param name="currentBankBalances">
        ///     The bank balances as at the reconciliation date to include in this new single line of the
        ///     ledger book.
        /// </param>
        /// <param name="budget">The current budget.</param>
        /// <param name="toDoList">
        ///     The task list that will have tasks added to it to remind the user to perform transfers and
        ///     payments etc.
        /// </param>
        /// <param name="statement">The currently loaded statement.</param>
        /// <param name="ignoreWarnings">Ignores validation warnings if true, otherwise <see cref="ValidationWarningException" />.</param>
        /// <exception cref="InvalidOperationException">Thrown when this <see cref="LedgerBook" /> is in an invalid state.</exception>
        internal virtual LedgerEntryLine Reconcile(
            DateTime reconciliationStartDate,
            IEnumerable<BankBalance> currentBankBalances,
            BudgetModel budget,
            ToDoCollection toDoList = null,
            StatementModel statement = null,
            bool ignoreWarnings = false)
        {
            // TODO Statement and ToDo list are not really optional.  Only unit tests do not pass in these values. 
            try
            {
                PreReconciliationValidation(reconciliationStartDate, statement);
            }
            catch (ValidationWarningException)
            {
                if (!ignoreWarnings)
                {
                    throw;
                }
            }

            if (toDoList == null)
            {
                toDoList = new ToDoCollection();
            }

            decimal consistencyCheck1 = Reconciliations.Sum(e => e.CalculatedSurplus);
            var newLine = this.reconciliationBuilder.CreateNewMonthlyReconciliation(reconciliationStartDate, currentBankBalances, budget, statement, toDoList);
            decimal consistencyCheck2 = Reconciliations.Sum(e => e.CalculatedSurplus);
            if (consistencyCheck1 != consistencyCheck2)
            {
                throw new CorruptedLedgerBookException("Code Error: The previous dated entries have changed, this is not allowed. Data is corrupt.");
            }

            this.reconciliations.Insert(0, newLine);

            return newLine;
        }

        /// <summary>
        ///     Deletes the most recent <see cref="LedgerEntryLine" /> from the <see cref="Reconciliations" /> collection. Only the
        ///     most recent one can be deleted and
        ///     only if it is unlocked (generally means just created and not yet saved).
        /// </summary>
        internal void RemoveLine([NotNull] LedgerEntryLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            if (!line.IsNew)
            {
                throw new InvalidOperationException("You cannot delete a Ledger Entry Line that is not unlocked or a newly created line.");
            }

            if (line != Reconciliations.FirstOrDefault())
            {
                throw new InvalidOperationException("You cannot delete this line, it is not the first and most recent line.");
            }

            this.reconciliations.Remove(line);
        }

        /// <summary>
        ///     Used to allow the UI to set a ledger's account, but only if it is an instance in the <see cref="Ledgers" />
        ///     collection.
        /// </summary>
        /// <param name="ledger"></param>
        /// <param name="storedInAccount"></param>
        internal void SetLedgerAccount(LedgerBucket ledger, Account.Account storedInAccount)
        {
            if (Ledgers.Any(l => l == ledger))
            {
                ledger.StoredInAccount = storedInAccount;
                return;
            }

            throw new InvalidOperationException("You cannot change the account in a ledger that is not in the Ledgers collection.");
        }

        internal void SetReconciliations(List<LedgerEntryLine> lines)
        {
            this.reconciliations = lines.OrderByDescending(l => l.Date).ToList();
        }

        /// <summary>
        ///     Used to unlock the most recent <see cref="LedgerEntryLine" />. Lines are locked as soon as they are saved after
        ///     creation to prevent changes.
        ///     Use with caution, this is intended to keep data integrity intact and prevent accidental changes. After financial
        ///     records are completed for
        ///     the month, they are not supposed to change.
        /// </summary>
        internal LedgerEntryLine UnlockMostRecentLine()
        {
            LedgerEntryLine line = Reconciliations.FirstOrDefault();
            line?.Unlock();

            return line;
        }

        private void PreReconciliationValidation(DateTime startDate, StatementModel statement)
        {
            var messages = new StringBuilder();
            if (!Validate(messages))
            {
                throw new InvalidOperationException("Ledger book is currently in an invalid state. Cannot add new entries.\n" + messages);
            }

            if (statement == null)
            {
                return;
            }

            ValidateDate(startDate, statement);

            ValidateAgainstUncategorisedTransactions(statement);

            ValidateAgainstOrphanedAutoMatchingTransactions(statement);
        }

        private void ValidateAgainstOrphanedAutoMatchingTransactions(StatementModel statement)
        {
            LedgerEntryLine lastLine = Reconciliations.FirstOrDefault();
            if (lastLine == null)
            {
                return;
            }

            List<LedgerTransaction> unmatchedTxns = lastLine.Entries
                .SelectMany(e => e.Transactions)
                .Where(t => !string.IsNullOrWhiteSpace(t.AutoMatchingReference) && !t.AutoMatchingReference.StartsWith(ReconciliationBuilder.MatchedPrefix, StringComparison.Ordinal))
                .ToList();

            if (unmatchedTxns.None())
            {
                return;
            }

            List<Transaction> statementSubSet = statement.AllTransactions.Where(t => t.Date >= lastLine.Date).ToList();
            foreach (LedgerTransaction ledgerTransaction in unmatchedTxns)
            {
                IEnumerable<Transaction> statementTxns = ReconciliationBuilder.TransactionsToAutoMatch(statementSubSet, ledgerTransaction.AutoMatchingReference);
                if (statementTxns.None())
                {
                    this.logger.LogWarning(
                        l =>
                            l.Format(
                                "There appears to be some transactions from last month that should be auto-matched to a statement transactions, but no matching statement transactions were found. {0}",
                                ledgerTransaction));
                    throw new ValidationWarningException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "There appears to be some transactions from last month that should be auto-matched to a statement transactions, but no matching statement transactions were found.\nHave you forgotten to do a transfer?\nTransaction ID:{0} Ref:{1} Amount:{2:C}",
                            ledgerTransaction.Id,
                            ledgerTransaction.AutoMatchingReference,
                            ledgerTransaction.Amount));
                }
            }
        }

        private void ValidateAgainstUncategorisedTransactions(StatementModel statement)
        {
            if (statement.AllTransactions.Any(t => t.BudgetBucket == null || (t.BudgetBucket != null && string.IsNullOrWhiteSpace(t.BudgetBucket.Code))))
            {
                IEnumerable<Transaction> uncategorised = statement.AllTransactions.Where(t => t.BudgetBucket == null || (t.BudgetBucket != null && string.IsNullOrWhiteSpace(t.BudgetBucket.Code)));
                var count = 0;
                this.logger.LogWarning(_ => "LedgerBook.PreReconciliationValidation: There appears to be transactions in the statement that are not categorised into a budget bucket.");
                foreach (Transaction transaction in uncategorised)
                {
                    count++;
                    Transaction transactionCopy = transaction;
                    this.logger.LogWarning(_ => "LedgerBook.PreReconciliationValidation: Transaction: " + transactionCopy.Id + transactionCopy.BudgetBucket);
                    if (count > 5)
                    {
                        this.logger.LogWarning(_ => "LedgerBook.PreReconciliationValidation: There are more than 5 transactions.");
                    }
                }

                throw new ValidationWarningException("There appears to be transactions in the statement that are not categorised into a budget bucket.");
            }
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void ValidateDate(DateTime date, StatementModel statement)
        {
            LedgerEntryLine recentEntry = Reconciliations.FirstOrDefault();
            if (recentEntry != null)
            {
                if (date <= recentEntry.Date)
                {
                    throw new InvalidOperationException("The start Date entered is before the previous ledger entry.");
                }

                if (recentEntry.Date.AddDays(7 * 4) > date)
                {
                    throw new InvalidOperationException("The start Date entered is not at least 4 weeks after the previous reconciliation. ");
                }

                if (recentEntry.Date.Day != date.Day)
                {
                    throw new ValidationWarningException(
                        "The start Date chosen, {0}, isn't the same day of the month as the previous entry {1}. Not required, but ideally reconciliations should be evenly spaced.");
                }
            }

            DateTime startDate = date.AddMonths(-1);
            if (!statement.AllTransactions.Any(t => t.Date >= startDate))
            {
                throw new ValidationWarningException("There doesn't appear to be any transactions in the statement for the month up to " + date.ToShortDateString());
            }
        }
    }
}