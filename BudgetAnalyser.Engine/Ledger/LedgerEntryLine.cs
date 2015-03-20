using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     This represents the horizontal row on the <see cref="LedgerBook" /> that crosses all <see cref="LedgerBucket" />s
    ///     for a date.
    ///     Each <see cref="LedgerEntry" /> must have a reference to an instance of this.
    /// </summary>
    public class LedgerEntryLine
    {
        public const string MatchedPrefix = "Matched ";
        private static readonly string[] DisallowedChars = { "\\", "{", "}", "[", "]", "^", "=" };
        private readonly ILogger logger;
        private List<BankBalanceAdjustmentTransaction> bankBalanceAdjustments = new List<BankBalanceAdjustmentTransaction>();
        private List<BankBalance> bankBalancesList;
        private List<LedgerEntry> entries = new List<LedgerEntry>();

        /// <summary>
        ///     Constructs a new instance of <see cref="LedgerEntryLine" />.
        ///     Only AutoMapper uses this constructor.  It it easier for AutoMapper configuration. Date and BankBalances are set
        ///     implicitly using the
        ///     private and internal setters.
        /// </summary>
        /// <param name="logger">The diagnostics logger</param>
        internal LedgerEntryLine([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
            IsNew = true;
        }

        /// <summary>
        ///     Constructs a new instance of <see cref="LedgerEntryLine" />.
        ///     Use this constructor for adding a new line when reconciling once a month.
        /// </summary>
        /// <param name="date">The date of the line</param>
        /// <param name="bankBalances">The bank balances for this date.</param>
        /// <param name="logger">The diagnostics logger</param>
        internal LedgerEntryLine(DateTime date, [NotNull] IEnumerable<BankBalance> bankBalances, [NotNull] ILogger logger)
            : this(logger)
        {
            if (bankBalances == null)
            {
                throw new ArgumentNullException("bankBalances");
            }

            Date = date;
            this.bankBalancesList = bankBalances.ToList();
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
            [UsedImplicitly] private set { this.entries = value.ToList(); }
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
        public decimal LedgerBalance
        {
            get { return TotalBankBalance + TotalBalanceAdjustments; }
        }

        public string Remarks { get; internal set; }

        /// <summary>
        ///     The individual surplus balance in each bank account being tracked by the Legder book.  These will add up to the
        ///     <see cref="CalculatedSurplus" />.
        /// </summary>
        public IEnumerable<BankBalance> SurplusBalances
        {
            get
            {
                IEnumerable<BankBalance> adjustedBalances = BankBalances.Select(b => new BankBalance(b.Account, b.Balance + TotalBankBalanceAdjustmentForAccount(b.Account)));
                IEnumerable<BankBalance> results = Entries.GroupBy(
                    e => e.LedgerBucket.StoredInAccount,
                    (accountType, ledgerEntries) => new BankBalance(accountType, ledgerEntries.Sum(e => e.Balance)));
                return adjustedBalances.Select(a => new BankBalance(a.Account, a.Balance - results.Where(r => r.Account == a.Account).Sum(r => r.Balance)));
            }
        }

        public decimal TotalBalanceAdjustments
        {
            get { return BankBalanceAdjustments.Sum(a => a.Amount); }
        }

        /// <summary>
        ///     Gets the total bank balance across all accounts. Does not include balance adjustments.
        /// </summary>
        public decimal TotalBankBalance
        {
            get { return this.bankBalancesList.Sum(b => b.Balance); }
        }

        /// <summary>
        ///     Called by <see cref="LedgerBook.Reconcile" />. It builds the contents of the new ledger line based on budget and
        ///     statement input.
        /// </summary>
        /// <param name="parentLedgerBook">
        ///     The parent Ledger Book.  Used to extract information from previous <see cref="LedgerEntry" />s to construct the
        ///     running
        ///     balance for the entries this line contains. Also used to get the LedgerBucket instance for the new Ledger Entries.
        ///     This is intentionally not necessarily the same as the previous Ledger Entry from last month, to allow the ledger to
        ///     be
        ///     transfered to a different bank account.
        /// </param>
        /// <param name="currentBudget">The current applicable budget</param>
        /// <param name="statement">The current period statement.</param>
        /// <param name="startDateIncl">
        ///     The date of the previous ledger line. This is used to include transactions from the
        ///     Statement up to but excluding the date of this reconciliation.
        /// </param>
        /// <param name="toDoList">The task list that will have tasks added to it to remind the user to perform transfers and payments etc.</param>
        internal void AddNew(
            LedgerBook parentLedgerBook,
            BudgetModel currentBudget,
            StatementModel statement,
            DateTime startDateIncl,
            ToDoCollection toDoList)
        {
            if (!IsNew)
            {
                throw new InvalidOperationException("Cannot add a new entry to an existing Ledger Line, only new Ledger Lines can have new entries added.");
            }

            DateTime finishDate = Date;
            // Date filter must include the start date, which goes back to and includes the previous ledger date up to the date of this ledger line, but excludes this ledger date.
            // For example if this is a reconciliation for the 20/Feb then the start date is 20/Jan and the finish date is 20/Feb. So transactions pulled from statement are between
            // 20/Jan (inclusive) and 19/Feb (inclusive).
            List<Transaction> filteredStatementTransactions = statement == null
                ? new List<Transaction>()
                : statement.AllTransactions.Where(t => t.Date >= startDateIncl && t.Date < finishDate).ToList();

            IEnumerable<LedgerEntry> previousLedgerBalances = CompileLedgersAndBalances(parentLedgerBook);

            foreach (LedgerEntry previousLedgerEntry in previousLedgerBalances)
            {
                LedgerBucket ledgerBucket = previousLedgerEntry.LedgerBucket;
                decimal openingBalance = previousLedgerEntry.Balance;
                var newEntry = new LedgerEntry(true) { Balance = openingBalance, LedgerBucket = ledgerBucket };
                List<LedgerTransaction> transactions = IncludeBudgetedAmount(currentBudget, ledgerBucket, finishDate, toDoList);
                transactions.AddRange(IncludeStatementTransactions(newEntry, filteredStatementTransactions));
                AutoMatchTransactionsAlreadyInPreviousPeriod(filteredStatementTransactions, previousLedgerEntry, transactions, toDoList);
                newEntry.SetTransactionsForReconciliation(transactions);

                this.entries.Add(newEntry);
            }
        }

        internal BankBalanceAdjustmentTransaction BalanceAdjustment(decimal adjustment, string narrative)
        {
            if (!IsNew)
            {
                throw new InvalidOperationException("Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            if (adjustment == 0)
            {
                throw new ArgumentException("The balance adjustment amount cannot be zero.", "adjustment");
            }

            var newAdjustment = new BankBalanceAdjustmentTransaction { Narrative = narrative, Amount = adjustment };

            this.bankBalanceAdjustments.Add(newAdjustment);
            return newAdjustment;
        }

        internal void CancelBalanceAdjustment(Guid transactionId)
        {
            if (!IsNew)
            {
                throw new InvalidOperationException("Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
            }

            BankBalanceAdjustmentTransaction txn = this.bankBalanceAdjustments.FirstOrDefault(t => t.Id == transactionId);
            if (txn != null)
            {
                this.bankBalanceAdjustments.Remove(txn);
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

        //internal void UpdateBankBalances(IEnumerable<BankBalance> updatedBankBalances)
        //{
        //    if (!IsNew)
        //    {
        //        throw new InvalidOperationException("You cannot update the bank balances for this ledger line.");
        //    }

        //    this.bankBalancesList = updatedBankBalances.ToList();
        //}

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
                throw new ArgumentNullException("validationMessages");
            }

            var result = true;

            if (Entries.None())
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "The Ledger Entry does not contain any entries, either delete it or add entries.");
                result = false;
            }

            decimal totalLedgers = Entries.Sum(e => e.Balance);
            if (totalLedgers + CalculatedSurplus - LedgerBalance != 0)
            {
                result = false;
                validationMessages.Append("All ledgers + surplus + balance adjustments does not equal balance.");
            }

            foreach (LedgerEntry ledgerEntry in Entries)
            {
                if (!ledgerEntry.Validate(validationMessages, FindPreviousEntryOpeningBalance(previousLine, ledgerEntry.LedgerBucket)))
                {
                    validationMessages.AppendFormat("Ledger Entry with Balance {0:C} is invalid.", ledgerEntry.Balance);
                    result = false;
                }
            }

            return result;
        }

        private void AutoMatchTransactionsAlreadyInPreviousPeriod(
            List<Transaction> transactions, 
            LedgerEntry previousLedgerEntry, 
            List<LedgerTransaction> newLedgerTransactions, 
            ToDoCollection toDoList)
        {
            List<LedgerTransaction> ledgerAutoMatchTransactions = previousLedgerEntry.Transactions.Where(t => !string.IsNullOrWhiteSpace(t.AutoMatchingReference)).ToList();
            var checkMatchedTxns = new List<LedgerTransaction>();
            var checkMatchCount = 0;
            foreach (LedgerTransaction lastMonthLedgerTransaction in ledgerAutoMatchTransactions)
            {
                this.logger.LogInfo(
                    l =>
                        l.Format(
                            "Ledger Reconciliation - AutoMatching - Found {0} {1} ledger transaction that require matching.",
                            ledgerAutoMatchTransactions.Count(),
                            previousLedgerEntry.LedgerBucket.BudgetBucket.Code));
                LedgerTransaction ledgerTxn = lastMonthLedgerTransaction;
                foreach (Transaction matchingStatementTransaction in TransactionsToAutoMatch(transactions, lastMonthLedgerTransaction.AutoMatchingReference))
                {
                    this.logger.LogInfo(l => l.Format("Ledger Reconciliation - AutoMatching - Matched {0} ==> {1}", ledgerTxn, matchingStatementTransaction));
                    ledgerTxn.Id = matchingStatementTransaction.Id;
                    if (!ledgerTxn.AutoMatchingReference.StartsWith(MatchedPrefix, StringComparison.Ordinal))
                    {
                        // There will be two statement transactions but only one ledger transaction to match to.
                        checkMatchCount++;
                        ledgerTxn.AutoMatchingReference = string.Format(CultureInfo.InvariantCulture, "{0}{1}", MatchedPrefix, ledgerTxn.AutoMatchingReference);
                        checkMatchedTxns.Add(ledgerTxn);
                    }

                    LedgerTransaction duplicateTransaction = newLedgerTransactions.FirstOrDefault(t => t.Id == matchingStatementTransaction.Id);
                    if (duplicateTransaction != null)
                    {
                        this.logger.LogInfo(l => l.Format("Ledger Reconciliation - Removing Duplicate Ledger transaction after auto-matching: {0}", duplicateTransaction));
                        newLedgerTransactions.Remove(duplicateTransaction);
                    }
                }
            }

            if (ledgerAutoMatchTransactions.Any() && ledgerAutoMatchTransactions.Count() != checkMatchCount)
            {
                this.logger.LogWarning(
                    l =>
                        l.Format(
                            "Ledger Reconciliation - WARNING {0} ledger transactions appear to be waiting to be automatched, but not statement transactions were found. {1}",
                            ledgerAutoMatchTransactions.Count(),
                            ledgerAutoMatchTransactions.First().AutoMatchingReference));
                var unmatchedTxns = ledgerAutoMatchTransactions.Except(checkMatchedTxns);
                foreach (var txn in unmatchedTxns)
                {
                    toDoList.Add(new ToDoTask(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "WARNING: Missing auto-match transaction. Transfer {0:C} with reference {1} Dated {2:d} to {3}. See log for more details.",
                            txn.Amount,
                            txn.AutoMatchingReference,
                            Date.AddDays(-1),
                            previousLedgerEntry.LedgerBucket.StoredInAccount), 
                        true));
                }
            }
        }

        private decimal TotalBankBalanceAdjustmentForAccount(AccountType account)
        {
            return BankBalanceAdjustments.Where(a => a.BankAccount == account).Sum(a => a.Amount);
        }

        internal static IEnumerable<Transaction> TransactionsToAutoMatch(IEnumerable<Transaction> transactions, string autoMatchingReference)
        {
            return transactions.Where(
                t =>
                    t.Reference1.TrimEndSafely() == autoMatchingReference
                    || t.Reference2.TrimEndSafely() == autoMatchingReference
                    || t.Reference3.TrimEndSafely() == autoMatchingReference)
                .OrderBy(t => t.Amount);
        }

        private static IEnumerable<LedgerEntry> CompileLedgersAndBalances(LedgerBook parentLedgerBook)
        {
            var ledgersAndBalances = new List<LedgerEntry>();
            LedgerEntryLine previousLine = parentLedgerBook.Reconciliations.FirstOrDefault();
            if (previousLine == null)
            {
                return parentLedgerBook.Ledgers.Select(ledger => new LedgerEntry { Balance = 0, LedgerBucket = ledger });
            }

            foreach (LedgerBucket ledger in parentLedgerBook.Ledgers)
            {
                // Ledger Columns from a previous are not necessarily equal if the StoredInAccount has changed.
                LedgerEntry previousEntry = previousLine.Entries.FirstOrDefault(e => e.LedgerBucket.BudgetBucket == ledger.BudgetBucket);

                // Its important to use the ledger column value from the book level map, not from the previous entry. The user
                // could have moved the ledger to a different account and so, the ledger column value in the book level map will be different.
                if (previousEntry == null)
                {
                    // Indicates a new ledger column has been added to the book starting this month.
                    ledgersAndBalances.Add(new LedgerEntry { Balance = 0, LedgerBucket = ledger });
                }
                else
                {
                    ledgersAndBalances.Add(previousEntry);
                }
            }

            return ledgersAndBalances;
        }

        private static string ExtractNarrative(Transaction t)
        {
            if (!string.IsNullOrWhiteSpace(t.Description))
            {
                return t.Description;
            }

            if (t.TransactionType != null)
            {
                return t.TransactionType.ToString();
            }

            return string.Empty;
        }

        private static decimal FindPreviousEntryOpeningBalance([CanBeNull] LedgerEntryLine previousLine, [NotNull] LedgerBucket ledgerBucket)
        {
            if (ledgerBucket == null)
            {
                throw new ArgumentNullException("ledgerBucket");
            }

            if (previousLine == null)
            {
                return 0;
            }
            LedgerEntry previousEntry = previousLine.Entries.FirstOrDefault(e => e.LedgerBucket == ledgerBucket);
            return previousEntry == null ? 0 : previousEntry.Balance;
        }

        private static List<LedgerTransaction> IncludeBudgetedAmount(BudgetModel currentBudget, LedgerBucket ledgerBucket, DateTime reconciliationDate, ToDoCollection toDoList)
        {
            Expense budgetedExpense = currentBudget.Expenses.FirstOrDefault(e => e.Bucket.Code == ledgerBucket.BudgetBucket.Code);
            var transactions = new List<LedgerTransaction>();
            if (budgetedExpense != null)
            {
                BudgetCreditLedgerTransaction budgetedAmount;
                if (ledgerBucket.StoredInAccount.IsSalaryAccount)
                {
                    budgetedAmount = new BudgetCreditLedgerTransaction
                    {
                        Amount = budgetedExpense.Bucket.Active ? budgetedExpense.Amount : 0,
                        Narrative = budgetedExpense.Bucket.Active ? "Budgeted Amount" : "Warning! Bucket has been disabled."
                    };
                }
                else
                {
                    budgetedAmount = new BudgetCreditLedgerTransaction
                    {
                        Amount = budgetedExpense.Bucket.Active ? budgetedExpense.Amount : 0,
                        Narrative =
                            budgetedExpense.Bucket.Active
                                ? "Budget amount must be transferred into this account with a bank transfer, use the reference number for the transfer."
                                : "Warning! Bucket has been disabled.",
                        AutoMatchingReference = IssueTransactionReferenceNumber()
                    };
                    toDoList.Add(
                        new ToDoTask(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "Transfer {0:C} from Salary Account to {1} with auto-matching reference: {2}",
                                budgetedAmount.Amount, 
                                ledgerBucket.StoredInAccount,
                                budgetedAmount.AutoMatchingReference), 
                            true));
                }

                budgetedAmount.Date = reconciliationDate;
                transactions.Add(budgetedAmount);
            }

            return transactions;
        }

        private static IEnumerable<LedgerTransaction> IncludeStatementTransactions(LedgerEntry newEntry, ICollection<Transaction> filteredStatementTransactions)
        {
            if (filteredStatementTransactions.None())
            {
                return new List<LedgerTransaction>();
            }

            List<Transaction> transactions = filteredStatementTransactions.Where(t => t.BudgetBucket == newEntry.LedgerBucket.BudgetBucket).ToList();
            if (transactions.Any())
            {
                IEnumerable<LedgerTransaction> newLedgerTransactions = transactions.Select<Transaction, LedgerTransaction>(
                    t =>
                    {
                        if (t.Amount < 0)
                        {
                            return new CreditLedgerTransaction(t.Id)
                            {
                                Amount = t.Amount,
                                Narrative = ExtractNarrative(t),
                                Date = t.Date
                            };
                        }

                        return new CreditLedgerTransaction(t.Id)
                        {
                            Amount = t.Amount,
                            Narrative = ExtractNarrative(t),
                            Date = t.Date
                        };
                    });

                return newLedgerTransactions.ToList();
            }

            return new List<LedgerTransaction>();
        }

        private static string IssueTransactionReferenceNumber()
        {
            var reference = new StringBuilder(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
            foreach (string disallowedChar in DisallowedChars)
            {
                reference.Replace(disallowedChar, string.Empty);
            }

            return reference.ToString().Substring(0, 7);
        }
    }
}