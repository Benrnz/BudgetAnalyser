using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

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
        public LedgerColumn LedgerColumn { get; internal set; }

        /// <summary>
        ///     The total net affect of all transactions in this entry.  Debits will be negative.
        /// </summary>
        public decimal NetAmount
        {
            get { return this.transactions.Sum(t => t.Amount); }
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
            var newBalance = Balance + (newTransaction.Amount);
            Balance = newBalance > 0 ? newBalance : 0;
            var balanceAdjustmentTransaction = newTransaction as BankBalanceAdjustmentTransaction;
            if (balanceAdjustmentTransaction != null)
            {
                balanceAdjustmentTransaction.BankAccount = LedgerColumn.StoredInAccount;
            }
        }

        public void RemoveTransaction(Guid transactionId)
        {
            if (!this.isNew)
            {
                throw new InvalidOperationException("Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            var txn = this.transactions.FirstOrDefault(t => t.Id == transactionId);
            if (txn != null)
            {
                this.transactions.Remove(txn);
                Balance -= txn.Amount;
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
                Amount = -Balance,
                Narrative = narrative ?? "Zeroing balance - excess funds in this account."
            };
            this.transactions.Add(zeroTxn);
            Balance = 0;
        }

        /// <summary>
        ///     Called by <see cref="LedgerBook.Reconcile" />. Sets up this new Entry with transactions.
        /// </summary>
        /// <param name="newTransactions">The list of new transactions for this entry. This includes the monthly budgeted amount.</param>
        internal void SetTransactionsForReconciliation(List<LedgerTransaction> newTransactions)
        {
            const string supplementOverdrawnText = "Automatically supplementing overdrawn balance from surplus";

            this.transactions = newTransactions;
            LedgerTransaction zeroingTransaction = null;
            if (LedgerColumn.BudgetBucket is SpentMonthlyExpenseBucket && NetAmount != 0)
            {
                // SpentMonthly ledgers automatically zero their balance. They dont accumulate nor can they be negative.
                // The balance does not need to be updated, it will always remain the same as the previous closing balance.
                if (NetAmount < 0)
                {
                    if (newTransactions.OfType<BudgetCreditLedgerTransaction>().Any())
                    {
                        // Ledger is still overdrawn despite the transactions including the new month's budgeted amount. Create a zeroing transaction so the sum total of all txns = 0.
                        // This way the ledger closing balance will be equal to the previous ledger closing balance.
                        zeroingTransaction = new CreditLedgerTransaction
                        {
                            Amount = -NetAmount,
                            Narrative = "SpentMonthlyLedger: " + supplementOverdrawnText
                        };
                    }
                    else
                    {
                        if (Balance + NetAmount < 0)
                        {
                            // This ledger does not have a monthly budgeted amount, so if all funds are gone, it must be zeroed.
                            zeroingTransaction = new CreditLedgerTransaction
                            {
                                Amount = -(Balance + NetAmount),
                                Narrative = "SpentMonthlyLedger: " + supplementOverdrawnText
                            };
                        }
                        else
                        {
                            // Some of the funds accumulated in this ledger have been spent, but a positive closing balance remains.
                            Balance += NetAmount;
                        }
                    }
                }
                else
                {
                    zeroingTransaction = new CreditLedgerTransaction
                    {
                        Amount = -NetAmount,
                        Narrative = "SpentMonthlyLedger: automatically zeroing the credit remainder"
                    };
                }
            }
            else
            {
                // All other ledgers can accumulate a balance but cannot be negative.
                var newBalance = Balance + NetAmount;
                Balance = newBalance < 0 ? 0 : newBalance;
                var budgetedAmount = this.transactions.FirstOrDefault(t => t is BudgetCreditLedgerTransaction);
                if (budgetedAmount != null && newBalance < budgetedAmount.Amount)
                {
                    // This ledger has a monthly budgeted amount and the balance has resulted in a balance less than the monthly budgeted amount, supplement from surplus to equal budgeted amount.
                    // While there is a monthly amount the balance should not drop below this amount.
                    zeroingTransaction = new CreditLedgerTransaction
                    {
                        Amount = budgetedAmount.Amount - newBalance,
                        Narrative = newBalance < 0 ? supplementOverdrawnText : "Automatically supplementing shortfall so balance is not less than monthly budget amount"
                    };
                }
                else if (newBalance < 0)
                {
                    zeroingTransaction = new CreditLedgerTransaction
                    {
                        Amount = -newBalance,
                        Narrative = supplementOverdrawnText
                    };
                }
            }

            if (zeroingTransaction != null)
            {
                this.transactions.Add(zeroingTransaction);
            }
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