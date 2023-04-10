using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

[AutoRegisterWithIoC(SingleInstance = true)]
internal class ReconciliationBuilder : IReconciliationBuilder
{
    internal const string MatchedPrefix = "Matched ";
    private readonly ILogger logger;
    private readonly IList<ToDoTask> toDoList = new List<ToDoTask>();
    private LedgerEntryLine newReconciliationLine;

    public ReconciliationBuilder([NotNull] ILogger logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public LedgerBook LedgerBook { get; set; }

    /// <summary>
    ///     This is effectively stage 2 of the Reconciliation process. Called by <see cref="ReconciliationBuilder.CreateNewMonthlyReconciliation" />. It builds the contents of the new
    ///     ledger line based on budget and statement input.
    /// </summary>
    /// <param name="reconciliationDateExclusive">
    /// The date for the reconciliation. This is typically pay date, and is the beginning date of the new period. Also one day after the global filter end date. Used to select transactions
    /// from the statement.</param>
    /// <param name="budget">The current applicable budget</param>
    /// <param name="statement">The current period statement.</param>
    /// <param name="bankBalances">A list of bank balances for the reconciliation, one for each account. (Excluding credit cards).</param>
    public ReconciliationResult CreateNewMonthlyReconciliation(
        DateTime reconciliationDateExclusive,
        BudgetModel budget,
        StatementModel statement,
        params BankBalance[] bankBalances)
    {
        if (bankBalances == null)
        {
            throw new ArgumentNullException(nameof(bankBalances));
        }

        if (budget == null)
        {
            throw new ArgumentNullException(nameof(budget));
        }

        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        if (LedgerBook == null)
        {
            throw new ArgumentException("The Ledger Book property cannot be null. You must set this prior to calling this method.");
        }

        try
        {
            this.newReconciliationLine = new LedgerEntryLine(reconciliationDateExclusive, bankBalances);
            AddNew(budget, statement, CalculateBeginDateForReconciliationPeriod(LedgerBook, reconciliationDateExclusive, budget.BudgetCycle));

            return new ReconciliationResult { Reconciliation = this.newReconciliationLine, Tasks = this.toDoList };
        }
        finally
        {
            this.newReconciliationLine = null;
        }
    }

    public static IEnumerable<LedgerTransaction> FindAutoMatchingTransactions([CanBeNull] LedgerEntryLine recon, bool includeMatchedTransactions = false)
    {
        if (recon == null)
        {
            return new List<LedgerTransaction>();
        }

        return recon.Entries.SelectMany(e => FindAutoMatchingTransactions(e, includeMatchedTransactions));
    }

    public static bool IsAutoMatchingTransaction(Transaction statementTransaction, IEnumerable<LedgerTransaction> ledgerTransactions)
    {
        return
            ledgerTransactions.Any(
                                   l =>
                                       l.AutoMatchingReference == statementTransaction.Reference1 ||
                                       l.AutoMatchingReference == $"{MatchedPrefix}{statementTransaction.Reference1}");
    }

    /// <summary>
    ///     When creating a new reconciliation a start date is required to be able to search a statement for transactions between the start date and the reconciliation date specified
    ///     (today or pay day). The start date should start from the previous ledger entry line or one month prior if no records exist.
    /// </summary>
    /// <param name="ledgerBook">The Ledger Book to find the date for</param>
    /// <param name="reconciliationDate">The chosen reconciliation date from the user</param>
    /// <param name="periodType">The budget period monthly/fortnightly</param>
    internal static DateTime CalculateBeginDateForReconciliationPeriod(LedgerBook ledgerBook, DateTime reconciliationDate, BudgetCycle periodType)
    {
        if (ledgerBook.Reconciliations.Any())
        {
            // If the LedgerBook has a previous entry, this new reconciliation must cover the period from this previous closing off date.
            return ledgerBook.Reconciliations.First().Date;
        }

        // Only used if this is the first reconciliation for this LedgerBook.
        switch (periodType)
        {
            case BudgetCycle.Monthly:
                return reconciliationDate.AddMonths(-1);
            case BudgetCycle.Fortnightly:
                return reconciliationDate.AddDays(-14);
            default:
                throw new NotSupportedException("Invalid budget period detected: " + periodType);
        }
    }

    internal static IEnumerable<Transaction> TransactionsToAutoMatch(IEnumerable<Transaction> transactions, string autoMatchingReference)
    {
        IOrderedEnumerable<Transaction> txns = transactions.Where(
                                                                  t =>
                                                                      t.Reference1.TrimEndSafely() == autoMatchingReference
                                                                      || t.Reference2.TrimEndSafely() == autoMatchingReference
                                                                      || t.Reference3.TrimEndSafely() == autoMatchingReference)
            .OrderBy(t => t.Amount);
        return txns;
    }

    private void AddNew(
        BudgetModel budget,
        StatementModel statement,
        DateTime startDateIncl)
    {
        if (!this.newReconciliationLine.IsNew)
        {
            throw new InvalidOperationException("Cannot add a new entry to an existing Ledger Line, only new Ledger Lines can have new entries added.");
        }

        var reconciliationDate = this.newReconciliationLine.Date;
        // Date filter must include the start date, which goes back to and includes the previous ledger date up to the date of this ledger line, but excludes this ledger date.
        // For example for a monthly budget if this is a reconciliation for the 20/Feb then the start date is 20/Jan and the finish date is 20/Feb. So transactions pulled from statement are between
        // 20/Jan (inclusive) and 19/Feb (inclusive) but not including anything for the 20th of Feb.
        // Why? Because the new ledger entry is intended to show the starting balances for the new period, so you can plan for the upcoming period.
        List<Transaction> filteredStatementTransactions = statement?.AllTransactions.Where(t => t.Date >= startDateIncl && t.Date < reconciliationDate).ToList() ?? new List<Transaction>();

        IEnumerable<LedgerEntry> previousLedgerBalances = CompileLedgersAndBalances(LedgerBook);

        var entries = new List<LedgerEntry>();
        foreach (var previousLedgerEntry in previousLedgerBalances)
        {
            LedgerBucket ledgerBucket;
            var openingBalance = previousLedgerEntry.Balance;
            var currentLedger = LedgerBook.Ledgers.Single(l => l.BudgetBucket == previousLedgerEntry.LedgerBucket.BudgetBucket);
            if (previousLedgerEntry.LedgerBucket.StoredInAccount != currentLedger.StoredInAccount)
            {
                // Check to see if a ledger has been moved into a new default account since last reconciliation.
                ledgerBucket = currentLedger;
            }
            else
            {
                ledgerBucket = previousLedgerEntry.LedgerBucket;
            }

            var newEntry = new LedgerEntry(true) { Balance = openingBalance, LedgerBucket = ledgerBucket };

            // Start by adding the budgeted amount to a list of transactions.
            List<LedgerTransaction> transactions = IncludeBudgetedAmount(budget, ledgerBucket, reconciliationDate);

            // Append all other transactions for this bucket, if any, to the transaction list.
            transactions.AddRange(IncludeStatementTransactions(newEntry, filteredStatementTransactions));

            AutoMatchTransactionsAlreadyInPreviousPeriod(filteredStatementTransactions, previousLedgerEntry, transactions);
            newEntry.SetTransactionsForReconciliation(transactions);

            entries.Add(newEntry);
        }

        this.newReconciliationLine.SetNewLedgerEntries(entries);

        foreach (var behaviour in ReconciliationBehaviourFactory.ListAllBehaviours())
        {
            behaviour.Initialise(filteredStatementTransactions,
                                 this.newReconciliationLine,
                                 this.toDoList,
                                 this.logger,
                                 statement);
            behaviour.ApplyBehaviour();
        }

        // At this point each ledger balance is still set to the opening balance, it hasn't ben updated yet. This should always be done last.
        foreach (var ledger in this.newReconciliationLine.Entries)
        {
            ledger.Balance += ledger.Transactions.Sum(t => t.Amount);
        }
    }

    /// <summary>
    ///     Match statement transaction with special automatching references to Ledger transactions. Configures hyperlinking
    ///     ids and marks then as matched. Also checks to ensure they are matched for data integrity.
    /// </summary>
    private void AutoMatchTransactionsAlreadyInPreviousPeriod(
        List<Transaction> transactions,
        LedgerEntry previousLedgerEntry,
        List<LedgerTransaction> newLedgerTransactions)
    {
        List<LedgerTransaction> ledgerAutoMatchTransactions = FindAutoMatchingTransactions(previousLedgerEntry).ToList();
        var checkMatchedTxns = new List<LedgerTransaction>();
        var checkMatchCount = 0;
        foreach (var lastMonthLedgerTransaction in ledgerAutoMatchTransactions)
        {
            this.logger.LogInfo(l => l.Format("Ledger Reconciliation - AutoMatching - Found {0} {1} ledger transaction that require matching.", ledgerAutoMatchTransactions.Count(),
                                              previousLedgerEntry.LedgerBucket.BudgetBucket.Code));

            var ledgerTxn = lastMonthLedgerTransaction;
            foreach (var matchingStatementTransaction in TransactionsToAutoMatch(transactions, lastMonthLedgerTransaction.AutoMatchingReference))
            {
                this.logger.LogInfo(l => l.Format("Ledger Reconciliation - AutoMatching - Matched {0} ==> {1}", ledgerTxn, matchingStatementTransaction));

                ledgerTxn.Id = matchingStatementTransaction.Id; // Allows user to click and link back to statement transaction.

                // Don't automatch if it has already been auto-matched
                if (!ledgerTxn.AutoMatchingReference.StartsWith(MatchedPrefix, StringComparison.Ordinal))
                {
                    // There will be two statement transactions but only one ledger transaction to match to.
                    checkMatchCount++;
                    ledgerTxn.AutoMatchingReference = $"{MatchedPrefix}{ledgerTxn.AutoMatchingReference}";
                    checkMatchedTxns.Add(ledgerTxn);
                }

                // Remove automatched transactions from the new recon
                var duplicateTransaction = newLedgerTransactions.FirstOrDefault(t => t.Id == matchingStatementTransaction.Id);
                if (duplicateTransaction != null)
                {
                    this.logger.LogInfo(l => l.Format("Ledger Reconciliation - Removing Duplicate Ledger transaction after auto-matching: {0}", duplicateTransaction));

                    newLedgerTransactions.Remove(duplicateTransaction);
                }
            }
        }

        // Check for any orphaned transactions that should have been auto-matched. This is for data integrity, it shouldn't happen.
        if (ledgerAutoMatchTransactions.Any() && ledgerAutoMatchTransactions.Count() != checkMatchCount)
        {
            this.logger.LogWarning(
                                   l =>
                                       l.Format(
                                                "Ledger Reconciliation - WARNING {0} ledger transactions appear to be waiting to be auto-matched, but no statement transactions were found. {1}",
                                                ledgerAutoMatchTransactions.Count(),
                                                ledgerAutoMatchTransactions.First().AutoMatchingReference));
            IEnumerable<LedgerTransaction> unmatchedTxns = ledgerAutoMatchTransactions.Except(checkMatchedTxns);
            foreach (var txn in unmatchedTxns)
            {
                this.toDoList.Add(
                                  new ToDoTask(
                                               string.Format(
                                                             CultureInfo.CurrentCulture,
                                                             "WARNING: Missing auto-match transaction. Transfer {0:C} with reference {1} Dated {2:d} to {3}. See log for more details.",
                                                             txn.Amount,
                                                             txn.AutoMatchingReference,
                                                             this.newReconciliationLine.Date.AddDays(-1),
                                                             previousLedgerEntry.LedgerBucket.StoredInAccount),
                                               true));
            }
        }
    }

    private static IEnumerable<LedgerEntry> CompileLedgersAndBalances(LedgerBook parentLedgerBook)
    {
        var ledgersAndBalances = new List<LedgerEntry>();
        var previousLine = parentLedgerBook.Reconciliations.FirstOrDefault();
        if (previousLine == null)
        {
            return parentLedgerBook.Ledgers.Select(ledger => new LedgerEntry { Balance = 0, LedgerBucket = ledger });
        }

        foreach (var ledger in parentLedgerBook.Ledgers)
        {
            // Ledger Columns from a previous are not necessarily equal if the StoredInAccount has changed.
            var previousEntry = previousLine.Entries.FirstOrDefault(e => e.LedgerBucket.BudgetBucket == ledger.BudgetBucket);

            // Its important to use the ledger column value from the book level map, not from the previous entry. The user
            // could have moved the ledger to a different account and so, the ledger column value in the book level map will be different.
            if (previousEntry == null)
            {
                // Indicates a new ledger column has been added to the book starting this month/fortnight.
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

    private static IEnumerable<LedgerTransaction> FindAutoMatchingTransactions(LedgerEntry ledgerEntry, bool includeMatchedTransactions = false)
    {
        if (ledgerEntry == null)
        {
            return new List<LedgerTransaction>();
        }

        if (includeMatchedTransactions)
        {
            return ledgerEntry.Transactions.Where(t => !string.IsNullOrWhiteSpace(t.AutoMatchingReference));
        }

        return
            ledgerEntry.Transactions.Where(
                                           t =>
                                               t.AutoMatchingReference.IsSomething() &&
                                               !t.AutoMatchingReference.StartsWith(MatchedPrefix, StringComparison.Ordinal));
    }

    private List<LedgerTransaction> IncludeBudgetedAmount(BudgetModel currentBudget, LedgerBucket ledgerBucket, DateTime reconciliationDate)
    {
        var budgetedExpense = currentBudget.Expenses.FirstOrDefault(e => e.Bucket.Code == ledgerBucket.BudgetBucket.Code);
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
                    Narrative = budgetedExpense.Bucket.Active
                        ? "Budget amount must be transferred into this account with a bank transfer, use the reference number for the transfer."
                        : "Warning! Bucket has been disabled.",
                    AutoMatchingReference = ReferenceNumberGenerator.IssueTransactionReferenceNumber()
                };
                // TODO Maybe the budget should know which account the incomes go into, perhaps mapped against each income?
                var salaryAccount = this.newReconciliationLine.BankBalances.Single(b => b.Account.IsSalaryAccount).Account;
                this.toDoList.Add(
                                  new TransferTask(
                                                   string.Format(
                                                                 CultureInfo.CurrentCulture,
                                                                 "Budgeted Amount for {0} transfer {1:C} from Salary Account to {2} with auto-matching reference: {3}",
                                                                 budgetedExpense.Bucket.Code,
                                                                 budgetedAmount.Amount,
                                                                 ledgerBucket.StoredInAccount,
                                                                 budgetedAmount.AutoMatchingReference),
                                                   true)
                                  {
                                      Amount = budgetedAmount.Amount,
                                      SourceAccount = salaryAccount,
                                      DestinationAccount = ledgerBucket.StoredInAccount,
                                      BucketCode = budgetedExpense.Bucket.Code,
                                      Reference = budgetedAmount.AutoMatchingReference
                                  });
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
            IEnumerable<LedgerTransaction> newLedgerTransactions = transactions.Select(
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
}