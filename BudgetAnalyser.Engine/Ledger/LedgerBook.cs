using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
    public class LedgerBook : IModelValidate
    {
        private readonly IReconciliationBuilder reconciliationBuilder;
        private List<LedgerBucket> ledgersColumns = new List<LedgerBucket>();
        private List<LedgerEntryLine> reconciliations;

        /// <summary>
        ///     Constructs a new instance of the <see cref="LedgerBook" /> class.  The Persistence system calls this constructor,
        ///     not the IoC system.
        /// </summary>
        public LedgerBook([NotNull] IReconciliationBuilder reconciliationBuilder)
        {
            if (reconciliationBuilder == null)
            {
                throw new ArgumentNullException(nameof(reconciliationBuilder));
            }
            this.reconciliationBuilder = reconciliationBuilder;
            this.reconciliationBuilder.LedgerBook = this;
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

        internal LedgerBucket AddLedger(ExpenseBucket budgetBucket, Account storeInThisAccount)
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
        /// <param name="reconciliationDate">
        ///     The startDate for the <see cref="LedgerEntryLine" />. This is usually the previous Month's "Reconciliation-Date",
        ///     as this month's reconciliation starts with this date and includes transactions
        ///     from that date. This date is different to the "Reconciliation-Date" that appears next to the resulting
        ///     reconciliation which is the end date for the period.
        /// </param>
        /// <param name="currentBankBalances">
        ///     The bank balances as at the reconciliation date to include in this new single line of the
        ///     ledger book.
        /// </param>
        /// <param name="budget">The current budget.</param>
        /// <param name="statement">The currently loaded statement.</param>
        internal virtual ReconciliationResult Reconcile(
            DateTime reconciliationDate,
            IEnumerable<BankBalance> currentBankBalances,
            BudgetModel budget,
            StatementModel statement)
        {
            ReconciliationResult newRecon = this.reconciliationBuilder.CreateNewMonthlyReconciliation(reconciliationDate, currentBankBalances, budget, statement);
            this.reconciliations.Insert(0, newRecon.Reconciliation);
            return newRecon;
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
        internal void SetLedgerAccount(LedgerBucket ledger, Account storedInAccount)
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
    }
}