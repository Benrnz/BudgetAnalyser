using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Mobile;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     The top level ledger object. Primarily it contains groupings of all reconciliations performed to date.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.IModelValidate" />
    public class LedgerBook : IModelValidate
    {
        private readonly IReconciliationBuilder reconciliationBuilder;
        private List<LedgerBucket> ledgersColumns = new List<LedgerBucket>();
        private List<LedgerEntryLine> reconciliations;

        internal LedgerBook() : this(new ReconciliationBuilder(new NullLogger()))
        {
            throw new NotSupportedException("This constructor is only used for producing mappers by reflection.");
        }

        /// <summary>
        ///     Constructs a new instance of the <see cref="LedgerBook" /> class.  The Persistence system calls this constructor,
        ///     not the IoC system.
        /// </summary>
        internal LedgerBook([NotNull] IReconciliationBuilder reconciliationBuilder)
        {
            if (reconciliationBuilder == null)
            {
                throw new ArgumentNullException(nameof(reconciliationBuilder));
            }
            this.reconciliationBuilder = reconciliationBuilder;
            this.reconciliationBuilder.LedgerBook = this;
            this.reconciliations = new List<LedgerEntryLine>();
        }

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

        /// <summary>
        ///     The configuration for the remote mobile data storage
        /// </summary>
        public MobileStorageSettings MobileSettings { get; internal set; }

        /// <summary>
        ///     Gets the last modified date.
        /// </summary>
        public DateTime Modified { get; internal set; }

        /// <summary>
        ///     Gets the name of this ledger book.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        ///     Gets the monthly reconciliations collection.
        /// </summary>
        public IEnumerable<LedgerEntryLine> Reconciliations
        {
            get { return this.reconciliations; }
            [UsedImplicitly] private set { this.reconciliations = value.ToList(); }
        }

        /// <summary>
        ///     Gets the storage key that uniquely identifies this ledger book for storage purposes.
        /// </summary>
        public string StorageKey { get; internal set; }

        /// <summary>
        ///     Validate the instance and populate any warnings and errors into the <paramref name="validationMessages" /> string
        ///     builder.
        /// </summary>
        /// <param name="validationMessages">A non-null string builder that will be appended to for any messages.</param>
        /// <returns>
        ///     If the instance is in an invalid state it will return false, otherwise it returns true.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public bool Validate([NotNull] StringBuilder validationMessages)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException(nameof(validationMessages));
            }

            if (string.IsNullOrWhiteSpace(StorageKey))
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "A ledger book must have a file name.");
                return false;
            }

            var line = Reconciliations.FirstOrDefault();
            if (line != null)
            {
                var previous = Reconciliations.Skip(1).FirstOrDefault();
                if (previous != null && line.Date <= previous.Date)
                {
                    validationMessages.AppendFormat(CultureInfo.CurrentCulture,
                        "Duplicate and or out of sequence dates exist in the reconciliations for this Ledger Book.");
                    return false;
                }

                if (!line.Validate(validationMessages, previous))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Lists all the Ledgers available for transfer funds to and from.
        /// </summary>
        public IEnumerable<LedgerBucket> LedgersAvailableForTransfer()
        {
            List<LedgerBucket> ledgers = Ledgers.ToList();
            IEnumerable<Account> accounts = Ledgers.Select(l => l.StoredInAccount).Distinct();
            foreach (var account in accounts)
            {
                ledgers.Insert(0, new SurplusLedger { StoredInAccount = account });
            }

            return ledgers;
        }

        internal LedgerBucket AddLedger(LedgerBucket newLedger)
        {
            if (this.ledgersColumns.Any(l => l.BudgetBucket == newLedger.BudgetBucket))
            {
                // Ledger already exists in this ledger book.
                return null;
            }

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
        /// <param name="budget">The current budget.</param>
        /// <param name="statement">The currently loaded statement.</param>
        /// <param name="currentBankBalances">
        ///     The bank balances as at the reconciliation date to include in this new single line of the
        ///     ledger book.
        /// </param>
        internal virtual ReconciliationResult Reconcile(DateTime reconciliationDate, BudgetModel budget,
                                                        StatementModel statement, params BankBalance[] currentBankBalances)
        {
            var newRecon = this.reconciliationBuilder.CreateNewMonthlyReconciliation(reconciliationDate, budget, statement, currentBankBalances);
            this.reconciliations.Insert(0, newRecon.Reconciliation);
            return newRecon;
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

            throw new InvalidOperationException(
                "You cannot change the account in a ledger that is not in the Ledgers collection.");
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
            var line = Reconciliations.FirstOrDefault();
            line?.Unlock();

            return line;
        }
    }
}