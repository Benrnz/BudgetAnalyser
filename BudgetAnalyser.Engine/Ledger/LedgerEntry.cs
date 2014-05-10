using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A single entry on a <see cref="LedgerColumn" /> for a date (which comes from the <see cref="LedgerEntryLine" />). This
    ///     instance can contain one or
    ///     more <see cref="LedgerTransaction" />s defining all movements for this <see cref="BudgetBucket" /> for this date.
    ///     Possible transactions
    ///     include budgeted 'saved up for expenses' credited into this <see cref="LedgerColumn" /> and all statement transactions
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
        internal LedgerEntry(LedgerColumn ledger, LedgerEntry previousLedgerEntry)
        {
            Balance = previousLedgerEntry == null ? 0 : previousLedgerEntry.Balance;
            this.LedgerColumn = ledger;
            this.transactions = new List<LedgerTransaction>();
        }

        /// <summary>
        ///     Used when adding a new entry for a new reconciliation.
        /// </summary>
        internal LedgerEntry(LedgerColumn ledger, LedgerEntry previousLedgerEntry, bool isNew)
        {
            Balance = previousLedgerEntry == null ? 0 : previousLedgerEntry.Balance;
            this.LedgerColumn = ledger;
            this.transactions = new List<LedgerTransaction>();
            this.isNew = isNew;
        }

        public decimal Balance { get; private set; }

        public LedgerColumn LedgerColumn { get; private set; }

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

        public void AddTransaction([NotNull] LedgerTransaction newTransaction)
        {
            if (newTransaction == null)
            {
                throw new ArgumentNullException("newTransaction");
            }

            this.transactions.Add(newTransaction);
            decimal newBalance = Balance + (newTransaction.Credit - newTransaction.Debit);
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

        /// <summary>
        ///     Called by <see cref="LedgerBook.Reconcile" />. Sets up this new Entry with transactions.
        ///     ALSO USED BY Persistence.
        /// </summary>
        /// <param name="newTransactions">The list of new transactions for this entry.</param>
        /// <param name="reconciliationMode">
        ///     Set this to true if performing a reconciliation and as a result adding this new Entry.
        ///     Set this to false, default, when rehydrating from file.
        /// </param>
        internal LedgerEntry SetTransactions(List<LedgerTransaction> newTransactions, bool reconciliationMode = false)
        {
            this.transactions = newTransactions;
            if (reconciliationMode && this.LedgerColumn.BudgetBucket is SpentMonthlyExpenseBucket && NetAmount != 0)
            {
                // SpentMonthly ledgers automatically zero their balance. They dont accumulate nor can they be negative.
                LedgerTransaction zeroingTransaction = null;
                if (NetAmount < 0)
                {
                    if (newTransactions.OfType<BudgetCreditLedgerTransaction>().Any())
                    {
                        zeroingTransaction = new CreditLedgerTransaction
                        {
                            Credit = -NetAmount,
                            Narrative = "SpentMonthlyLedger: automatically supplementing shortfall from surplus",
                        };
                    }
                    else
                    {
                        if (Balance + NetAmount < 0)
                        {
                            zeroingTransaction = new CreditLedgerTransaction
                            {
                                Credit = -(Balance + NetAmount),
                                Narrative = "SpentMonthlyLedger: automatically supplementing shortfall from surplus",
                            };
                        }
                    }
                }
                else
                {
                    zeroingTransaction = new DebitLedgerTransaction
                    {
                        Debit = NetAmount,
                        Narrative = "SpentMonthlyLedger: automatically zeroing the credit remainder",
                    };
                }

                if (zeroingTransaction != null)
                {
                    this.transactions.Add(zeroingTransaction);
                }
            }
            else
            {
                // All other ledgers can accumulate a balance but cannot be negative.
                decimal newBalance = Balance + NetAmount;
                Balance = newBalance < 0 ? 0 : newBalance;
            }

            return this;
        }
    }
}