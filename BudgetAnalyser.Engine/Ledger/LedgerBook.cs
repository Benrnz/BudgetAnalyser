using System;
using System.Collections.Generic;
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
        private readonly ILogger logger;
        private readonly List<LedgerColumn> newlyAddedLedgers = new List<LedgerColumn>();
        private List<LedgerEntryLine> datedEntries;

        public LedgerBook([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
            this.datedEntries = new List<LedgerEntryLine>();
        }

        public IEnumerable<LedgerEntryLine> DatedEntries
        {
            get { return this.datedEntries; }
            [UsedImplicitly]
            private set { this.datedEntries = value.ToList(); }
        }

        public string FileName { get; internal set; }

        public IEnumerable<LedgerColumn> Ledgers
        {
            get
            {
                return this.datedEntries.SelectMany(e => e.Entries)
                    .Select(e => e.LedgerColumn)
                    .Union(this.newlyAddedLedgers)
                    .Distinct()
                    .OrderBy(l => l.BudgetBucket.Code);
            }
        }

        public DateTime Modified { get; internal set; }
        public string Name { get; set; }

        public void AddLedger(ExpenseBucket budgetBucket)
        {
            if (Ledgers.Any(l => l.BudgetBucket == budgetBucket))
            {
                // Ledger already exists in this ledger book.
                return;
            }

            this.newlyAddedLedgers.Add(new LedgerColumn { BudgetBucket = budgetBucket });
        }

        /// <summary>
        ///     Creates a new LedgerEntryLine for this <see cref="LedgerBook" />.
        /// </summary>
        /// <param name="date">
        ///     The date for the <see cref="LedgerEntryLine" />. Also used to search for transactions in the
        ///     <see cref="statement" />.
        /// </param>
        /// <param name="bankBalances">
        ///     The bank balances as at the <see cref="date" /> to include in this new single line of the
        ///     ledger book.
        /// </param>
        /// <param name="budget">The current budget.</param>
        /// <param name="statement">The currently loaded statement.</param>
        /// <param name="ignoreWarnings">Ignores validation warnings if true, otherwise <see cref="ValidationWarningException" />.</param>
        /// <exception cref="InvalidOperationException">Thrown when this <see cref="LedgerBook" /> is in an invalid state.</exception>
        public LedgerEntryLine Reconcile(
            DateTime date,
            IEnumerable<BankBalance> bankBalances,
            BudgetModel budget,
            StatementModel statement = null,
            bool ignoreWarnings = false)
        {
            try
            {
                PreReconciliationValidation(date, statement);
            }
            catch (ValidationWarningException)
            {
                if (!ignoreWarnings)
                {
                    throw;
                }
            }

            decimal consistencyCheck1 = DatedEntries.Sum(e => e.CalculatedSurplus);
            var newLine = new LedgerEntryLine(date, bankBalances);
            var previousEntries = new Dictionary<LedgerColumn, LedgerEntry>();
            LedgerEntryLine previousLine = this.datedEntries.FirstOrDefault();
            foreach (LedgerColumn ledger in Ledgers)
            {
                LedgerEntry previousEntry = null;
                if (previousLine != null)
                {
                    previousEntry = previousLine.Entries.FirstOrDefault(e => e.LedgerColumn.Equals(ledger));
                }

                previousEntries.Add(ledger, previousEntry);
            }

            newLine.AddNew(previousEntries, budget, statement, CalculateStartDateForReconcile(date));
            decimal consistencyCheck2 = DatedEntries.Sum(e => e.CalculatedSurplus);
            if (consistencyCheck1 != consistencyCheck2)
            {
                throw new CorruptedLedgerBookException("Code Error: The previous dated entries have changed, this is not allowed. Data is corrupt.");
            }

            this.datedEntries.Insert(0, newLine);
            this.newlyAddedLedgers.Clear();
            return newLine;
        }

        public void RemoveLine([NotNull] LedgerEntryLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            if (!line.IsNew)
            {
                throw new InvalidOperationException("You cannot delete a Ledger Entry Line that is not unlocked or a newly created line.");
            }

            if (line != DatedEntries.FirstOrDefault())
            {
                throw new InvalidOperationException("You cannot delete this line, it is not the first and most recent line.");
            }

            this.datedEntries.Remove(line);
        }

        public LedgerEntryLine UnlockMostRecentLine()
        {
            LedgerEntryLine line = DatedEntries.FirstOrDefault();
            if (line != null)
            {
                line.Unlock();
            }

            return line;
        }

        public bool Validate([NotNull] StringBuilder validationMessages)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException("validationMessages");
            }

            if (string.IsNullOrWhiteSpace(FileName))
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "A ledger book must have a file name.");
                return false;
            }

            DateTime last = DateTime.MaxValue;
            foreach (LedgerEntryLine line in this.datedEntries)
            {
                DateTime thisDate = line.Date;
                if (thisDate >= last)
                {
                    validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Duplicate and or out of sequence dates exist in the dated entries for this Ledger Book.");
                    return false;
                }

                last = thisDate;
                if (!line.Validate(validationMessages))
                {
                    return false;
                }
            }

            return true;
        }

        internal void SetDatedEntries(List<LedgerEntryLine> lines)
        {
            this.datedEntries = lines.OrderByDescending(l => l.Date).ToList();
        }

        /// <summary>
        ///     When creating a new reconciliation a start date is required to be able to search a statement for transactions
        ///     between a start date and
        ///     the date specified (today or pay day). The start date should start from the previous ledger entry line or one month
        ///     prior if no records
        ///     exist.
        /// </summary>
        /// <param name="date">The chosen date from the user</param>
        private DateTime CalculateStartDateForReconcile(DateTime date)
        {
            if (DatedEntries.Any())
            {
                return DatedEntries.First().Date;
            }
            DateTime startDateIncl = date.AddMonths(-1);
            return startDateIncl;
        }

        private void PreReconciliationValidation(DateTime date, StatementModel statement)
        {
            var messages = new StringBuilder();
            if (!Validate(messages))
            {
                throw new InvalidOperationException("Ledger book is currently in an invalid state. Cannot add new entries.\n" + messages);
            }

            LedgerEntryLine recentEntry = DatedEntries.FirstOrDefault();
            if (recentEntry != null && date <= recentEntry.Date)
            {
                throw new InvalidOperationException("The date entered is before the previous ledger entry.");
            }

            if (statement == null)
            {
                return;
            }

            DateTime startDate = date.AddMonths(-1);
            if (!statement.AllTransactions.Any(t => t.Date >= startDate))
            {
                throw new ValidationWarningException("There doesn't appear to be any transactions in the statement for the month up to " + date.ToShortDateString());
            }

            if (statement.AllTransactions.Any(t => t.BudgetBucket == null || (t.BudgetBucket != null && string.IsNullOrWhiteSpace(t.BudgetBucket.Code))))
            {
                IEnumerable<Transaction> uncategorised = statement.AllTransactions.Where(t => t.BudgetBucket == null || (t.BudgetBucket != null && string.IsNullOrWhiteSpace(t.BudgetBucket.Code)));
                int count = 0;
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
    }
}
