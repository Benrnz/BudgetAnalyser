using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A single entry on a <see cref="Ledger" /> for a date (which comes from the <see cref="LedgerEntryLine" />). This
    ///     instance can contain one or
    ///     more <see cref="LedgerTransaction" />s defining all movements for this <see cref="BudgetBucket" /> for this date.
    ///     Possible transactions
    ///     include budgeted 'saved up for expenses' credited into this <see cref="Ledger" /> and all statement transactions
    ///     that are debitted to this
    ///     budget bucket ledger.
    /// </summary>
    public class LedgerEntry
    {
        /// <summary>
        ///     A variable to keep track if this is a newly created entry for a new reconciliation as opposed to creation from
        ///     loading from file.
        ///     This variable is intentionally not persisted.
        /// </summary>
        private readonly bool isNew;

        private List<LedgerTransaction> transactions;

        /// <summary>
        ///     Used only by persistence.
        /// </summary>
        internal LedgerEntry(Ledger ledger, LedgerEntry previousLedgerEntry)
        {
            Balance = previousLedgerEntry == null ? 0 : previousLedgerEntry.Balance;
            Ledger = ledger;
            this.transactions = new List<LedgerTransaction>();
        }

        /// <summary>
        ///     Used when adding a new entry for a new reconciliation.
        /// </summary>
        internal LedgerEntry(Ledger ledger, LedgerEntry previousLedgerEntry, bool isNew)
        {
            Balance = previousLedgerEntry == null ? 0 : previousLedgerEntry.Balance;
            Ledger = ledger;
            this.transactions = new List<LedgerTransaction>();
            this.isNew = true;
        }

        public decimal Balance { get; private set; }

        public Ledger Ledger { get; private set; }

        /// <summary>
        ///     The total net affect of all transactions in this entry.  Debits will be negative.
        /// </summary>
        public decimal NetAmount
        {
            get { return this.transactions.Sum(t => t.Credit - t.Debit); }
        }

        public IEnumerable<LedgerTransaction> Transactions
        {
            get { return this.transactions; }
        }

        public void AddTransaction(LedgerTransaction newTransaction)
        {
            this.transactions.Add(newTransaction);
            var newBalance = Balance + (newTransaction.Credit - newTransaction.Debit);
            Balance = newBalance > 0 ? newBalance : 0;
        }

        public void RemoveTransaction(Guid transactionId)
        {
            if (!this.isNew)
            {
                throw new InvalidOperationException("Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            LedgerTransaction txn = this.transactions.FirstOrDefault(t => t.Id == transactionId);
            if (txn != null)
            {
                this.transactions.Remove(txn);
                Balance -= (txn.Credit - txn.Debit);
            }
        }

        /// <summary>
        ///     Use this method to remove all funds from this ledger. This is commonly used periodically if overbudgeted and funds
        ///     can be safely used elsewhere.
        ///     By zeroing the balance the surplus will increase.
        /// </summary>
        public void ZeroTheBalance(string narrative = null)
        {
            if (!this.isNew)
            {
                throw new InvalidOperationException("This is not a new entry and therefore cannot be altered. Only newly created entries from creating a new reconciliation can use this operation.");
            }

            if (Balance <= 0)
            {
                return;
            }

            var zeroTxn = new BudgetCreditLedgerTransaction { Debit = Balance, Narrative = narrative ?? "Zeroing balance - excess funds in this account." };
            this.transactions.Add(zeroTxn);
            Balance = 0;
        }

        internal LedgerEntry SetTransactions(List<LedgerTransaction> newTransactions)
        {
            this.transactions = newTransactions;
            decimal newBalance = Balance + NetAmount;
            Balance = newBalance < 0 ? 0 : newBalance;
            return this;
        }
    }
}