using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A single entry on a <see cref="BudgetBucket" /> for a date (which comes from the <see cref="LedgerEntryLine" />).
    ///     This
    ///     instance can contain one or
    ///     more <see cref="LedgerTransaction" />s defining all movements for this <see cref="Budget.BudgetBucket" /> for this
    ///     date.
    ///     Possible transactions
    ///     include budgeted 'saved up for expenses' credited into this <see cref="BudgetBucket" /> and all statement
    ///     transactions
    ///     that are debitted to this
    ///     budget bucket ledger.
    /// </summary>
    public class LedgerEntry
    {
        /// <summary>
        ///     A variable to keep track if this is a newly created entry for a new reconciliation as opposed to creation from
        ///     loading from file.  An entry can be 'unlocked', this allows editing of this entry after it has been saved and
        ///     reloaded. Only the most recent entries for a <see cref="LedgerEntryLine" /> can be unlocked.
        ///     This variable is intentionally not persisted.
        /// </summary>
        private bool isNew;

        private List<LedgerTransaction> transactions;

        /// <summary>
        ///     Used only by persistence.
        /// </summary>
        internal LedgerEntry()
        {
            this.transactions = new List<LedgerTransaction>();
        }

        /// <summary>
        ///     Used when adding a new entry for a new reconciliation.
        /// </summary>
        internal LedgerEntry(bool isNew)
            : this()
        {
            this.isNew = isNew;
        }

        /// <summary>
        ///     The balance of the ledger as at the date after the transactions are applied in the parent
        ///     <see cref="LedgerEntryLine" />.
        /// </summary>
        public decimal Balance { get; internal set; }

        /// <summary>
        ///     The Ledger Column instance that tracks which <see cref="BudgetBucket" /> is being tracked by this Ledger.
        ///     This will also designate which Bank Account the ledger funds are stored.
        ///     Note that this may be different to the master mapping in <see cref="LedgerBook.Ledgers" />. This is because this
        ///     instance shows which account stored the funds at the date in the parent <see cref="LedgerEntryLine" />.
        /// </summary>
        public LedgerBucket LedgerBucket { get; internal set; }

        /// <summary>
        ///     The total net affect of all transactions in this entry.  Debits will be negative.
        /// </summary>
        public decimal NetAmount => this.transactions.Sum(t => t.Amount);

        /// <summary>
        ///     Gets the transactions collection for this entry.
        /// </summary>
        public IEnumerable<LedgerTransaction> Transactions => this.transactions;

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Ledger Entry - {0}", LedgerBucket);
        }

        /// <summary>
        ///     Used for persistence only.  Don't use during Reconciliation.
        /// </summary>
        /// <param name="newTransaction"></param>
        internal void AddTransaction([NotNull] LedgerTransaction newTransaction)
        {
            if (newTransaction == null)
            {
                throw new ArgumentNullException(nameof(newTransaction));
            }

            this.transactions.Add(newTransaction);
            var newBalance = Balance + newTransaction.Amount;
            Balance = newBalance > 0 ? newBalance : 0;
            var balanceAdjustmentTransaction = newTransaction as BankBalanceAdjustmentTransaction;
            if (balanceAdjustmentTransaction != null)
            {
                balanceAdjustmentTransaction.BankAccount = LedgerBucket.StoredInAccount;
            }
        }

        internal void Lock()
        {
            this.isNew = false;
        }

        internal void RemoveTransaction(Guid transactionId)
        {
            if (!this.isNew)
            {
                throw new InvalidOperationException(
                    "Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            var txn = this.transactions.FirstOrDefault(t => t.Id == transactionId);
            if (txn != null)
            {
                this.transactions.Remove(txn);
                Balance -= txn.Amount;
            }
        }

        /// <summary>
        ///     Called by <see cref="LedgerBook.Reconcile" />. Sets up this new Entry with transactions.
        ///     <see cref="AddTransaction" /> must not be called in conjunction with this.
        ///     This is used for reconciliation only.
        ///     Also performs some automated actions:
        ///     + Transfers to Surplus any remaining amount for Spent Monthly Buckets.
        ///     + Transfers from Surplus any overdrawn amount for Spent Monthly Buckets.
        /// </summary>
        /// <param name="newTransactions">The list of new transactions for this entry. This includes the monthly budgeted amount.</param>
        /// <param name="reconciliationDate">
        ///     The reconciliation date - this is used to give automatically created transactions a
        ///     date.
        /// </param>
        internal void SetTransactionsForReconciliation(List<LedgerTransaction> newTransactions,
                                                       DateTime reconciliationDate)
        {
            if (this.transactions.Any())
            {
                throw new InvalidOperationException("Code Error: You cannot call Set-Transactions-For-Reconciliation on an existing entry that already has transactions.");
            }

            LedgerBucket.ApplyReconciliationBehaviour(newTransactions, reconciliationDate, Balance);
            this.transactions = newTransactions.OrderBy(t => t.Date).ToList();
            Balance += NetAmount;
        }

        internal void Unlock()
        {
            this.isNew = true;
        }

        internal bool Validate(StringBuilder validationMessages, decimal openingBalance)
        {
            var result = true;
            if (LedgerBucket == null)
            {
                validationMessages.AppendLine("Ledger Bucket cannot be null on " + this);
                return false;
            }

            if (LedgerBucket.BudgetBucket == null)
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture,
                    "Ledger Bucket '{0}' has no Bucket assigned.", LedgerBucket);
                result = false;
            }

            if (openingBalance + Transactions.Sum(t => t.Amount) != Balance)
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture,
                    "Ledger Entry '{0}' transactions do not add up to the calculated balance!", this);
                result = false;
            }

            return result;
        }
    }
}