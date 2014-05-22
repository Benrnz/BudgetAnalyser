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
        private List<LedgerEntry> entries = new List<LedgerEntry>();

        /// <summary>
        ///     A variable to keep track if this is a newly created entry for a new reconciliation as opposed to creation from
        ///     loading from file.
        ///     This variable is intentionally not persisted.
        /// </summary>
        private bool isNew;

        private readonly List<BankBalance> bankBalancesList;

        /// <summary>
        ///     Constructs a new instance of <see cref="LedgerEntryLine" />. This constructor is used by deserialisation.
        /// </summary>
        /// <param name="date">The date of the line</param>
        /// <param name="bankBalances">The bank balances for this date. The sum of which makes up the total bank balance for this entry line.</param>
        /// <param name="remarks">The remarks saved with this line.</param>
        internal LedgerEntryLine(DateTime date, IEnumerable<BankBalance> bankBalances, string remarks)
        {
            this.bankBalancesList = bankBalances.ToList();
            Date = date;
            Remarks = remarks;
        }

        /// <summary>
        ///     Constructs a new instance of <see cref="LedgerEntryLine" />. Use this constructor for adding a new line when
        ///     reconciling once a month.
        /// </summary>
        /// <param name="date">The date of the line</param>
        /// <param name="bankBalances">The bank balances for this date.</param>
        internal LedgerEntryLine(DateTime date, IEnumerable<BankBalance> bankBalances)
        {
            this.isNew = true;
            Date = date;
            this.bankBalancesList = bankBalances.ToList();
        }

        public IEnumerable<BankBalance> BankBalances
        {
            get { return this.bankBalancesList; }
        }

        public decimal TotalBankBalance
        {
            get { return this.bankBalancesList.Sum(b => b.Balance); }
        }

        public IEnumerable<LedgerTransaction> BankBalanceAdjustments
        {
            get { return this.bankBalanceAdjustments; }
        }

        public decimal CalculatedSurplus
        {
            get { return LedgerBalance - Entries.Sum(e => e.Balance); }
        }

        public DateTime Date { get; private set; }

        public IEnumerable<LedgerEntry> Entries
        {
            get { return this.entries; }
        }

        public decimal LedgerBalance
        {
            get { return TotalBankBalance + TotalBalanceAdjustments; }
        }

        public string Remarks { get; private set; }

        public decimal TotalBalanceAdjustments
        {
            get { return BankBalanceAdjustments.Sum(a => a.Credit - a.Debit); }
        }

        public void BalanceAdjustment(decimal adjustment, string narrative)
        {
            if (!this.isNew)
            {
                throw new InvalidOperationException("Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            double amount = Convert.ToDouble(adjustment);
            if (Math.Abs(amount) < 0.01)
            {
                return;
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
        }

        public void CancelBalanceAdjustment(Guid transactionId)
        {
            if (!this.isNew)
            {
                throw new InvalidOperationException("Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            LedgerTransaction txn = this.bankBalanceAdjustments.FirstOrDefault(t => t.Id == transactionId);
            if (txn != null)
            {
                this.bankBalanceAdjustments.Remove(txn);
            }
        }

        public bool UpdateRemarks(string remarks)
        {
            if (this.isNew)
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

            if (!Entries.Any())
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "The Ledger Entry does not contain any entries, either delete it or add entries.");
                return false;
            }

            return true;
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
            if (!this.isNew)
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
                var newEntry = new LedgerEntry(ledger, previousEntry.Value, true);
                Expense expenseBudget = currentBudget.Expenses.FirstOrDefault(e => e.Bucket.Code == ledger.BudgetBucket.Code);
                var transactions = new List<LedgerTransaction>();
                if (expenseBudget != null)
                {
                    var budgetedAmount = new BudgetCreditLedgerTransaction { Credit = expenseBudget.Amount, Narrative = "Budgeted Amount" };
                    transactions.Add(budgetedAmount);
                }

                transactions.AddRange(IncludeStatementTransactions(newEntry, filteredStatementTransactions));

                newEntry.SetTransactions(transactions, true);
                this.entries.Add(newEntry);
            }
        }

        /// <summary>
        ///     Used by the mapper to map from Data entitiy to Domain entity, ie used indirectly by deserialisation.
        /// </summary>
        internal LedgerEntryLine SetBalanceAdjustments(List<LedgerTransaction> balanceAdjustments)
        {
            this.bankBalanceAdjustments = balanceAdjustments;
            return this;
        }

        /// <summary>
        ///     Used by the mapper to map from Data entitiy to Domain entity, ie used indirectly by deserialisation.
        /// </summary>
        internal LedgerEntryLine SetEntries(List<LedgerEntry> replacementEntries)
        {
            this.entries = replacementEntries;
            return this;
        }

        internal void Unlock()
        {
            this.isNew = true;
            foreach (LedgerEntry entry in Entries)
            {
                entry.Unlock();
            }
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
                IEnumerable<DebitLedgerTransaction> newLedgerTransactions = transactions.Select(t => new DebitLedgerTransaction
                {
                    Debit = -t.Amount, // Statement debits are negative, I want them to be positive here unless they are debit reversals where they should be negative.
                    Narrative = t.Description,
                });

                return newLedgerTransactions.ToList();
            }

            return new List<LedgerTransaction>();
        }
    }
}