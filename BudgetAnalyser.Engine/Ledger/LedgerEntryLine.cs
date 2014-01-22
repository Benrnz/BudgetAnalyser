using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     This represents the horizontal row on the <see cref="LedgerBook" /> that crosses all <see cref="Ledger" />s for a date.
    ///     Each <see cref="LedgerEntry" /> must have a reference to an instance of this.
    /// </summary>
    public class LedgerEntryLine : IModelValidate
    {
        /// <summary>
        ///     A variable to keep track if this is a newly created entry for a new reconciliation as opposed to creation from loading from file.
        ///     This variable is intentionally not persisted.
        /// </summary>
        private readonly bool isNew;

        private List<LedgerTransaction> bankBalanceAdjustments = new List<LedgerTransaction>();
        private List<LedgerEntry> entries = new List<LedgerEntry>();

        /// <summary>
        ///     Constructs a new instance of <see cref="LedgerEntryLine" />. This constructor is used by deserialisation.
        /// </summary>
        /// <param name="date">The date of the line</param>
        /// <param name="bankBalance">The bank balance for this date.</param>
        /// <param name="remarks">The remarks saved with this line.</param>
        internal LedgerEntryLine(DateTime date, decimal bankBalance, string remarks)
        {
            BankBalance = bankBalance;
            Date = date;
            Remarks = remarks;
        }

        /// <summary>
        ///     Constructs a new instance of <see cref="LedgerEntryLine" />. Use this constructor for adding a new line when reconciling once a month.
        /// </summary>
        /// <param name="date">The date of the line</param>
        /// <param name="bankBalance">The bank balance for this date.</param>
        internal LedgerEntryLine(DateTime date, decimal bankBalance)
        {
            this.isNew = true;
            Date = date;
            BankBalance = bankBalance;
        }

        public decimal BankBalance { get; private set; }

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
            get { return BankBalance + TotalBalanceAdjustments; }
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

        public bool Validate(StringBuilder validationMessages)
        {
            if (!Entries.Any())
            {
                validationMessages.AppendFormat("The Ledger Entry does not contain any entries, either delete it or add entries.");
                return false;
            }

            return true;
        }

        internal void AddNew(IDictionary<Ledger, LedgerEntry> previousEntries, BudgetModel currentBudget, StatementModel statement, DateTime startDateIncl)
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
                Ledger ledger = previousEntry.Key;
                var newEntry = new LedgerEntry(ledger, previousEntry.Value, true);
                Expense expenseBudget = currentBudget.Expenses.FirstOrDefault(e => e.Bucket.Code == ledger.BudgetBucket.Code);
                var transactions = new List<LedgerTransaction>();
                if (expenseBudget != null)
                {
                    var budgetedAmount = new BudgetCreditLedgerTransaction { Credit = expenseBudget.Amount, Narrative = "Budgeted Amount" };
                    transactions.Add(budgetedAmount);
                }

                transactions.AddRange(IncludeStatementTransactions(newEntry, filteredStatementTransactions));

                newEntry.SetTransactions(transactions);
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

        private IEnumerable<LedgerTransaction> IncludeStatementTransactions(LedgerEntry newEntry, ICollection<Transaction> filteredStatementTransactions)
        {
            if (!filteredStatementTransactions.Any())
            {
                return new List<LedgerTransaction>();
            }

            List<Transaction> transactions = filteredStatementTransactions.Where(t => t.BudgetBucket == newEntry.Ledger.BudgetBucket).ToList();
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