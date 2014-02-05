using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    public class LedgerBook : IModelValidate
    {
        private readonly List<Ledger> newlyAddedLedgers = new List<Ledger>();
        private List<LedgerEntryLine> datedEntries;

        public LedgerBook(string name, DateTime modified, string fileName)
        {
            Name = name;
            Modified = modified;
            FileName = fileName;
            this.datedEntries = new List<LedgerEntryLine>();
        }

        public IEnumerable<LedgerEntryLine> DatedEntries
        {
            get { return this.datedEntries; }
        }

        public string FileName { get; private set; }

        public IEnumerable<Ledger> Ledgers
        {
            get
            {
                return this.datedEntries.SelectMany(e => e.Entries)
                           .Select(e => e.Ledger)
                           .Union(this.newlyAddedLedgers)
                           .Distinct();
            }
        }

        public DateTime Modified { get; private set; }
        public string Name { get; set; }

        public void AddLedger(ExpenseBudgetBucket budgetBucket)
        {
            if (Ledgers.Any(l => l.BudgetBucket == budgetBucket))
            {
                // Ledger already exists in this ledger book.
                return;
            }

            this.newlyAddedLedgers.Add(new Ledger { BudgetBucket = budgetBucket });
        }

        /// <summary>
        /// Creates a new LedgerEntryLine for this <see cref="LedgerBook"/>.
        /// </summary>
        /// <param name="date">The date for the <see cref="LedgerEntryLine"/>. Also used to search for transactions in the <see cref="statement"/>.</param>
        /// <param name="bankBalance">The bank balance as at the <see cref="date"/>.</param>
        /// <param name="budget">The current budget.</param>
        /// <param name="statement">The currently loaded statement.</param>
        /// <param name="ignoreWarnings">Ignores validation warnings if true, otherwise <see cref="ValidationWarningException"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown when this <see cref="LedgerBook"/> is in an invalid state.</exception>
        public LedgerEntryLine Reconcile(
            DateTime date, 
            decimal bankBalance, 
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

            var newLine = new LedgerEntryLine(date, bankBalance);
            var previousEntries = new Dictionary<Ledger, LedgerEntry>();
            LedgerEntryLine previousLine = this.datedEntries.FirstOrDefault();
            foreach (Ledger ledger in Ledgers)
            {
                LedgerEntry previousEntry = null;
                if (previousLine != null)
                {
                    previousEntry = previousLine.Entries.FirstOrDefault(e => e.Ledger.Equals(ledger));
                }

                previousEntries.Add(ledger, previousEntry);
            }

            newLine.AddNew(previousEntries, budget, statement, CalculateStartDateForReconcile(date));
            this.datedEntries.Insert(0, newLine);
            this.newlyAddedLedgers.Clear();
            return newLine;
        }

        /// <summary>
        /// When creating a new reconciliation a start date is required to be able to search a statement for transactions between a start date and
        /// the date specified (today or pay day). The start date should start from the previous ledger entry line or one month prior if no records
        /// exist.
        /// </summary>
        /// <param name="date">The chosen date from the user</param>
        private DateTime CalculateStartDateForReconcile(DateTime date)
        {
            if (DatedEntries.Any())
            {
                return DatedEntries.First().Date;
            }
            else
            {
                DateTime startDateIncl = date.AddMonths(-1);
                return startDateIncl;
            }
        }

        private void PreReconciliationValidation(DateTime date, StatementModel statement)
        {
            var messages = new StringBuilder();
            if (!Validate(messages))
            {
                throw new InvalidOperationException("Ledger book is currently in an invalid state. Cannot add new entries.\n" + messages);
            }

            var recentEntry = DatedEntries.FirstOrDefault();
            if (recentEntry != null && date < recentEntry.Date)
            {
                throw new InvalidOperationException("The date entered is before the previous ledger entry.");
            }

            if (statement == null)
            {
                return;
            }

            var startDate = date.AddMonths(-1);
            if (!statement.AllTransactions.Any(t => t.Date >= startDate))
            {
                throw new ValidationWarningException("There doesn't appear to be any transactions in the statement for the month up to " + date.ToShortDateString());
            }
        }

        public bool Validate(StringBuilder validationMessages)
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                validationMessages.AppendFormat("A ledger book must have a file name.");
                return false;
            }

            DateTime last = DateTime.MaxValue;
            foreach (LedgerEntryLine line in this.datedEntries)
            {
                DateTime thisDate = line.Date;
                if (thisDate >= last)
                {
                    validationMessages.AppendFormat("Duplicate and or out of sequence dates exist in the dated entries for this Ledger Book.");
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
    }
}