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
    /// <summary>
    ///     This represents the horizontal row on the <see cref="LedgerBook" /> that crosses all <see cref="LedgerColumn" />s
    ///     for a
    ///     date.
    ///     Each <see cref="LedgerEntry" /> must have a reference to an instance of this.
    /// </summary>
    public class LedgerEntryLine : IModelValidate
    {
        private List<LedgerTransaction> bankBalanceAdjustments = new List<LedgerTransaction>();
        private List<BankBalance> bankBalancesList;
        private List<LedgerEntry> entries = new List<LedgerEntry>();

        /// <summary>
        ///     Constructs a new instance of <see cref="LedgerEntryLine" />. 
        ///     Use this constructor for adding a new line when reconciling once a month.
        /// </summary>
        /// <param name="date">The date of the line</param>
        /// <param name="bankBalances">The bank balances for this date.</param>
        internal LedgerEntryLine(DateTime date, IEnumerable<BankBalance> bankBalances)
        {
            IsNew = true;
            Date = date;
            this.bankBalancesList = bankBalances.ToList();
        }

        public IEnumerable<LedgerTransaction> BankBalanceAdjustments
        {
            get { return this.bankBalanceAdjustments; }
            [UsedImplicitly]
            private set { this.bankBalanceAdjustments = value.ToList(); }
        }

        public IEnumerable<BankBalance> BankBalances
        {
            get { return this.bankBalancesList; }
            [UsedImplicitly]
            private set { this.bankBalancesList = value.ToList(); }
        }

        public decimal CalculatedSurplus
        {
            get { return LedgerBalance - Entries.Sum(e => e.Balance); }
        }

        /// <summary>
        ///     This is the "as-at" date. It is the date of the fixed snapshot in time when this reconciliation line was created.
        ///     It is not editable as it is used to match transactions from the statement.  Changing this date would mean all
        ///     transactions
        ///     now falling outside the date range would need to be removed, thus affected balances.
        /// </summary>
        public DateTime Date { get; internal set; }

        public IEnumerable<LedgerEntry> Entries
        {
            get { return this.entries; }
            [UsedImplicitly]
            private set { this.entries = value.ToList(); }
        }

        public decimal LedgerBalance
        {
            get { return TotalBankBalance + TotalBalanceAdjustments; }
        }

        public string Remarks { get; internal set; }

        public decimal TotalBalanceAdjustments
        {
            get { return BankBalanceAdjustments.Sum(a => a.Credit - a.Debit); }
        }

        public decimal TotalBankBalance
        {
            get { return this.bankBalancesList.Sum(b => b.Balance); }
        }

        /// <summary>
        ///     A variable to keep track if this is a newly created entry for a new reconciliation as opposed to creation from
        ///     loading from file.
        ///     This variable is intentionally not persisted.
        ///     AutoMapper always sets this to false.
        ///     When a LedgerBook is saved the whole book is reloaded which will set this to false.
        /// </summary>
        internal bool IsNew { get; private set; }

        public LedgerTransaction BalanceAdjustment(decimal adjustment, string narrative)
        {
            if (!IsNew)
            {
                throw new InvalidOperationException("Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            if (adjustment == 0)
            {
                throw new ArgumentException("The balance adjustment amount cannot be zero.", "adjustment");
            }

            LedgerTransaction newAdjustment;
            if (adjustment < 0)
            {
                newAdjustment = new DebitLedgerTransaction
                {
                    Debit = -adjustment,
                    Narrative = narrative,
                };
            }
            else
            {
                newAdjustment = new CreditLedgerTransaction
                {
                    Credit = adjustment,
                    Narrative = narrative,
                };
            }

            this.bankBalanceAdjustments.Add(newAdjustment);
            return newAdjustment;
        }

        public void CancelBalanceAdjustment(Guid transactionId)
        {
            if (!IsNew)
            {
                throw new InvalidOperationException("Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            LedgerTransaction txn = this.bankBalanceAdjustments.FirstOrDefault(t => t.Id == transactionId);
            if (txn != null)
            {
                this.bankBalanceAdjustments.Remove(txn);
            }
        }

        public void UpdateBankBalances(IEnumerable<BankBalance> updatedBankBalances)
        {
            if (!IsNew)
            {
                throw new InvalidOperationException("You cannot update the bank balances for this ledger line.");
            }

            this.bankBalancesList = updatedBankBalances.ToList();
        }

        public bool UpdateRemarks(string remarks)
        {
            if (IsNew)
            {
                Remarks = remarks;
                return true;
            }

            return false;
        }

        public bool Validate([NotNull] StringBuilder validationMessages)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException("validationMessages");
            }

            bool result = true;

            if (!Entries.Any())
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "The Ledger Entry does not contain any entries, either delete it or add entries.");
                result = false;
            }

            foreach (var ledgerEntry in Entries)
            {
                if (!ledgerEntry.Validate())
                {
                    validationMessages.AppendFormat("Ledger Entry with Balance {0:C} is invalid.", ledgerEntry.Balance);
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        ///     Called by <see cref="LedgerBook.Reconcile" />. It builds the contents of the new ledger line based on budget and
        ///     statement input.
        /// </summary>
        /// <param name="previousEntries">
        ///     A collection of previous <see cref="LedgerEntry" />s to construct the running balance for
        ///     the entries this line contains.
        /// </param>
        /// <param name="currentBudget">The current applicable budget</param>
        /// <param name="statement">The current period statement.</param>
        /// <param name="startDateIncl">The date for this ledger line.</param>
        internal void AddNew(IEnumerable<KeyValuePair<LedgerColumn, LedgerEntry>> previousEntries, BudgetModel currentBudget, StatementModel statement, DateTime startDateIncl)
        {
            if (!IsNew)
            {
                throw new InvalidOperationException("Cannot add a new entry to an existing Ledger Line, only new Ledger Lines can have new entries added.");
            }

            DateTime finishDateExcl = Date;
            List<Transaction> filteredStatementTransactions = statement == null
                ? new List<Transaction>()
                : statement.AllTransactions.Where(t => t.Date >= startDateIncl && t.Date < finishDateExcl).ToList();
            foreach (var previousEntry in previousEntries)
            {
                LedgerColumn ledger = previousEntry.Key;
                decimal balance = previousEntry.Value == null ? 0 : previousEntry.Value.Balance;
                var newEntry = new LedgerEntry(true) { Balance = balance, LedgerColumn = ledger };
                Expense expenseBudget = currentBudget.Expenses.FirstOrDefault(e => e.Bucket.Code == ledger.BudgetBucket.Code);
                var transactions = new List<LedgerTransaction>();
                if (expenseBudget != null)
                {
                    var budgetedAmount = new BudgetCreditLedgerTransaction { Credit = expenseBudget.Amount, Narrative = "Budgeted Amount" };
                    transactions.Add(budgetedAmount);
                }

                transactions.AddRange(IncludeStatementTransactions(newEntry, filteredStatementTransactions));

                newEntry.SetTransactionsForReconciliation(transactions);
                this.entries.Add(newEntry);
            }
        }

        internal void Unlock()
        {
            IsNew = true;
            foreach (LedgerEntry entry in Entries)
            {
                entry.Unlock();
            }
        }

        private static string ExtractNarrative(Transaction t)
        {
            if (string.IsNullOrWhiteSpace(t.Description))
            {
                return t.Description;
            }

            if (t.TransactionType != null)
            {
                return t.TransactionType.ToString();
            }

            return string.Empty;
        }

        private static IEnumerable<LedgerTransaction> IncludeStatementTransactions(LedgerEntry newEntry, ICollection<Transaction> filteredStatementTransactions)
        {
            if (!filteredStatementTransactions.Any())
            {
                return new List<LedgerTransaction>();
            }

            List<Transaction> transactions = filteredStatementTransactions.Where(t => t.BudgetBucket == newEntry.LedgerColumn.BudgetBucket).ToList();
            if (transactions.Any())
            {
                IEnumerable<LedgerTransaction> newLedgerTransactions = transactions.Select<Transaction, LedgerTransaction>(
                    t =>
                    {
                        if (t.Amount < 0)
                        {
                            return new DebitLedgerTransaction(t.Id)
                            {
                                BankAccount = t.AccountType,
                                Debit = -t.Amount, // Statement debits are negative, I want them to be positive here unless they are debit reversals where they should be negative.
                                Narrative = ExtractNarrative(t),
                            };
                        }

                        return new CreditLedgerTransaction(t.Id)
                        {
                            BankAccount = t.AccountType,
                            Credit = t.Amount,
                            Narrative = ExtractNarrative(t),
                        };
                    });

                return newLedgerTransactions.ToList();
            }

            return new List<LedgerTransaction>();
        }
    }
}