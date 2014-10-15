using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A single entry on a <see cref="BudgetBucket" /> for a date (which comes from the <see cref="LedgerEntryLine" />).
    ///     This
    ///     instance can contain one or
    ///     more <see cref="LedgerTransaction" />s defining all movements for this <see cref="Budget.BudgetBucket" /> for this date.
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
        internal LedgerEntry(bool isNew) : this()
        {
            this.isNew = isNew;
        }

        /// <summary>
        /// The balance of the ledger as at the date after the transactions are applied in the parent <see cref="LedgerEntryLine"/>.
        /// </summary>
        public decimal Balance { get; internal set; }

        /// <summary>
        /// The Ledger Column instance that tracks which <see cref="BudgetBucket"/> is being tracked by this Ledger.
        /// This will also designate which Bank Account the ledger funds are stored.
        /// Note that this may be different to the master mapping in <see cref="LedgerBook.Ledgers"/>. This is because this 
        /// instance shows which account stored the funds at the date in the parent <see cref="LedgerEntryLine"/>.
        /// </summary>
        public LedgerColumn LedgerColumn { get; internal set; }

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
            [UsedImplicitly] private set { this.transactions = value.ToList(); }
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
            newTransaction.BankAccount = LedgerColumn.StoredInAccount;
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

            var zeroTxn = new BudgetCreditLedgerTransaction
            {
                Debit = Balance, 
                Narrative = narrative ?? "Zeroing balance - excess funds in this account.",
                BankAccount = LedgerColumn.StoredInAccount,
            };
            this.transactions.Add(zeroTxn);
            Balance = 0;
        }

        /// <summary>
        ///     Called by <see cref="LedgerBook.Reconcile" />. Sets up this new Entry with transactions.
        /// </summary>
        /// <param name="newTransactions">The list of new transactions for this entry.</param>
        internal LedgerEntry SetTransactionsForReconciliation(List<LedgerTransaction> newTransactions)
        {
            this.transactions = newTransactions;
            if (LedgerColumn.BudgetBucket is SpentMonthlyExpenseBucket && NetAmount != 0)
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
                            BankAccount = LedgerColumn.StoredInAccount,
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
                                BankAccount = LedgerColumn.StoredInAccount,
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
                        BankAccount = LedgerColumn.StoredInAccount,
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

        internal void Unlock()
        {
            this.isNew = true;
        }

        internal bool Validate()
        {
            if (LedgerColumn == null)
            {
                return false;
            }

            if (LedgerColumn.BudgetBucket == null)
            {
                return false;
            }

            return true;
        }
    }
}