using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.BankAccount;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     This represents a reconciliation as at a date in the <see cref="LedgerBook" /> that crosses all
    ///     <see cref="LedgerBucket" />s
    ///     for the reconciliation date.  It shows the financial position at a point in time, the reconciliation date.
    ///     An instance of this class contains many <see cref="LedgerEntry" />s show the financial position of that
    ///     <see cref="LedgerBucket" /> as at the reconciliation date.
    /// </summary>
    public class LedgerEntryLine
    {
        private List<BankBalanceAdjustmentTransaction> bankBalanceAdjustments =
            new List<BankBalanceAdjustmentTransaction>();

        private List<BankBalance> bankBalancesList;
        private List<LedgerEntry> entries = new List<LedgerEntry>();

        /// <summary>
        ///     Constructs a new instance of <see cref="LedgerEntryLine" />.
        ///     Only AutoMapper uses this constructor.  It it easier for AutoMapper configuration. Date and BankBalances are set
        ///     implicitly using the
        ///     private and internal setters.
        /// </summary>
        internal LedgerEntryLine()
        {
            IsNew = true;
        }

        /// <summary>
        ///     Constructs a new instance of <see cref="LedgerEntryLine" />.
        ///     Use this constructor for adding a new line when reconciling once a month.
        /// </summary>
        /// <param name="reconciliationDate">The date of the line</param>
        /// <param name="bankBalances">The bank balances for this date.</param>
        internal LedgerEntryLine(DateTime reconciliationDate, [NotNull] IEnumerable<BankBalance> bankBalances)
            : this()
        {
            if (bankBalances == null)
            {
                throw new ArgumentNullException(nameof(bankBalances));
            }

            Date = reconciliationDate;
            this.bankBalancesList = bankBalances.ToList();
            if (this.bankBalancesList.None())
            {
                throw new ArgumentException("There are no bank balances in the collection provided.",
                    nameof(bankBalances));
            }
        }

        /// <summary>
        ///     A collection of optional adjustments to the bank balance that can be added during a reconciliation.
        ///     This is to compensate for transactions that may not have been reflected in the bank account at the time of the
        ///     reconciliation.
        ///     Most commonly this is a credit card payment once the user has ascertained how much surplus they have.
        /// </summary>
        public IEnumerable<BankBalanceAdjustmentTransaction> BankBalanceAdjustments
        {
            get { return this.bankBalanceAdjustments; }
            [UsedImplicitly] private set { this.bankBalanceAdjustments = value.ToList(); }
        }

        /// <summary>
        ///     The bank balances of all the bank accounts being tracked by the ledger book.
        ///     These balances do not include balance adjustments.
        /// </summary>
        public IEnumerable<BankBalance> BankBalances
        {
            get { return this.bankBalancesList; }
            [UsedImplicitly] private set { this.bankBalancesList = value.ToList(); }
        }

        /// <summary>
        ///     The total surplus as at the given date.  This is the total surplus across all the bank accounts being tracked by
        ///     the ledger book.
        ///     This is the amount of money left over after funds have been allocated to all budget buckets being tracked by the
        ///     ledger entries.
        /// </summary>
        public decimal CalculatedSurplus => LedgerBalance - Entries.Sum(e => e.Balance);

        /// <summary>
        ///     This is the "as-at" date. It is the date of the fixed snapshot in time when this reconciliation line was created.
        ///     It is not editable as it is used to match transactions from the statement.  Changing this date would mean all
        ///     transactions
        ///     now falling outside the date range would need to be removed, thus affected balances.
        /// </summary>
        [UsedImplicitly]
        public DateTime Date { get; internal set; }

        /// <summary>
        ///     Gets the entries for all the Ledger Buckets.
        /// </summary>
        public IEnumerable<LedgerEntry> Entries
        {
            get { return this.entries; }
            private set { this.entries = value.ToList(); }
        }

        /// <summary>
        ///     A variable to keep track if this is a newly created entry for a new reconciliation as opposed to creation from
        ///     loading from file.
        ///     This variable is intentionally not persisted.
        ///     AutoMapper always sets this to false.
        ///     When a LedgerBook is saved the whole book is reloaded which will set this to false.
        /// </summary>
        internal bool IsNew { get; private set; }

        /// <summary>
        ///     Gets the grand total ledger balance. This includes a total of all accounts and all balance adjustments.
        /// </summary>
        public decimal LedgerBalance => TotalBankBalance + TotalBalanceAdjustments;

        /// <summary>
        ///     Gets the user remarks for this reconciliation.
        /// </summary>
        public string Remarks { get; internal set; }

        /// <summary>
        ///     The individual surplus balance in each bank account being tracked by the Legder book.  These will add up to the
        ///     <see cref="CalculatedSurplus" />.
        /// </summary>
        public IEnumerable<BankBalance> SurplusBalances
        {
            get
            {
                IEnumerable<BankBalance> adjustedBalances =
                    BankBalances.Select(
                        b => new BankBalance(b.Account, b.Balance + TotalBankBalanceAdjustmentForAccount(b.Account)));
                IEnumerable<BankBalance> results = Entries.GroupBy(
                    e => e.LedgerBucket.StoredInAccount,
                    (accountType, ledgerEntries) => new BankBalance(accountType, ledgerEntries.Sum(e => e.Balance)));
                return
                    adjustedBalances.Select(
                        a =>
                            new BankBalance(a.Account,
                                a.Balance - results.Where(r => r.Account == a.Account).Sum(r => r.Balance)));
            }
        }

        /// <summary>
        ///     Gets the calculated total balance adjustments.
        /// </summary>
        public decimal TotalBalanceAdjustments => BankBalanceAdjustments.Sum(a => a.Amount);

        /// <summary>
        ///     Gets the total bank balance across all accounts. Does not include balance adjustments.
        /// </summary>
        public decimal TotalBankBalance => this.bankBalancesList.Sum(b => b.Balance);

        internal BankBalanceAdjustmentTransaction BalanceAdjustment(decimal adjustment, string narrative,
                                                                    Account account)
        {
            if (!IsNew)
            {
                throw new InvalidOperationException(
                    "Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            if (adjustment == 0)
            {
                throw new ArgumentException("The balance adjustment amount cannot be zero.", nameof(adjustment));
            }

            var newAdjustment = new BankBalanceAdjustmentTransaction
            {
                Date = Date,
                Narrative = narrative,
                Amount = adjustment,
                BankAccount = account
            };

            this.bankBalanceAdjustments.Add(newAdjustment);
            return newAdjustment;
        }

        internal void CancelBalanceAdjustment(Guid transactionId)
        {
            if (!IsNew)
            {
                throw new InvalidOperationException(
                    "Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            var txn = this.bankBalanceAdjustments.FirstOrDefault(t => t.Id == transactionId);
            if (txn != null)
            {
                this.bankBalanceAdjustments.Remove(txn);
            }
        }

        internal void Lock()
        {
            IsNew = false;
            foreach (var entry in Entries)
            {
                entry.Lock();
            }
        }

        /// <summary>
        ///     Sets the <see cref="LedgerEntry" /> list for this reconciliation. Used when building a new reconciliation and
        ///     populating a new <see cref="LedgerEntryLine" />.
        /// </summary>
        internal void SetNewLedgerEntries(IEnumerable<LedgerEntry> ledgerEntries)
        {
            Entries = ledgerEntries.ToList();
        }

        internal void Unlock()
        {
            IsNew = true;
            foreach (var entry in Entries)
            {
                entry.Unlock();
            }
        }

        internal bool UpdateRemarks(string remarks)
        {
            if (IsNew)
            {
                Remarks = remarks;
                return true;
            }

            return false;
        }

        internal bool Validate([NotNull] StringBuilder validationMessages, [CanBeNull] LedgerEntryLine previousLine)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException(nameof(validationMessages));
            }

            var result = true;

            if (Entries.None())
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture,
                    "The Ledger Entry does not contain any entries, either delete it or add entries.");
                result = false;
            }

            var totalLedgers = Entries.Sum(e => e.Balance);
            if (totalLedgers + CalculatedSurplus - LedgerBalance != 0)
            {
                result = false;
                validationMessages.Append("All ledgers + surplus + balance adjustments does not equal balance.");
            }

            foreach (var ledgerEntry in Entries)
            {
                if (
                    !ledgerEntry.Validate(validationMessages,
                        FindPreviousEntryOpeningBalance(previousLine, ledgerEntry.LedgerBucket)))
                {
                    validationMessages.AppendFormat(CultureInfo.CurrentCulture,
                        "\nLedger Entry with Balance {0:C} is invalid.", ledgerEntry.Balance);
                    result = false;
                }
            }

            return result;
        }

        private static decimal FindPreviousEntryOpeningBalance([CanBeNull] LedgerEntryLine previousLine,
                                                               [NotNull] LedgerBucket ledgerBucket)
        {
            if (ledgerBucket == null)
            {
                throw new ArgumentNullException(nameof(ledgerBucket));
            }

            if (previousLine == null)
            {
                return 0;
            }

            var previousEntry =
                previousLine.Entries.FirstOrDefault(e => e.LedgerBucket.BudgetBucket == ledgerBucket.BudgetBucket);
            return previousEntry?.Balance ?? 0;
        }

        private decimal TotalBankBalanceAdjustmentForAccount(Account account)
        {
            return BankBalanceAdjustments.Where(a => a.BankAccount == account).Sum(a => a.Amount);
        }
    }
}