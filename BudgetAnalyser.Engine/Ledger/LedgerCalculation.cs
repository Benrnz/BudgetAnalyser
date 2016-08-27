using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A calculator that performs common calculations on a Ledger Book.
    /// </summary>
    [AutoRegisterWithIoC]
    public class LedgerCalculation
    {
        /// <summary>
        ///     A temporary cache with a short timeout to store results from <see cref="CalculateOverspentLedgers" />. This method
        ///     is called multiple times over a short period to build a burn down report.
        ///     I did consider using a MemoryCache here, but I don't like the fact it is IDisposable.
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> CalculationsCache =
            new ConcurrentDictionary<string, object>();

        private static DateTime CacheLastUpdated;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LedgerCalculation" /> class.
        /// </summary>
        public LedgerCalculation()
        {
            this.logger = new NullLogger();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LedgerCalculation" /> class.
        /// </summary>
        public LedgerCalculation(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        ///     Calculates the current month bucket spend.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public virtual decimal CalculateCurrentMonthBucketSpend(
            [NotNull] LedgerBook ledgerBook,
            [NotNull] GlobalFilterCriteria filter, 
            [NotNull] StatementModel statement, 
            [NotNull] string bucketCode)
        {
            CheckCacheForCleanUp();
            if (ledgerBook == null)
            {
                throw new ArgumentNullException(nameof(ledgerBook));
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (bucketCode.IsNothing())
            {
                throw new ArgumentNullException(nameof(bucketCode));
            }

            if (filter.BeginDate == null)
            {
                return 0;
            }

            var entryLine = LocateApplicableLedgerLine(ledgerBook, filter);
            var transactionTotal = CalculateTransactionTotal(filter.BeginDate.Value, statement, entryLine, bucketCode);
            return transactionTotal;
        }

        /// <summary>
        ///     Calculates the current month ledger balances.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public virtual IDictionary<BudgetBucket, decimal> CalculateCurrentMonthLedgerBalances(
            [NotNull] LedgerBook ledgerBook,
            [NotNull] GlobalFilterCriteria filter,
            [NotNull] StatementModel statement)
        {
            CheckCacheForCleanUp();
            if (ledgerBook == null)
            {
                throw new ArgumentNullException(nameof(ledgerBook));
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            var ledgers = new Dictionary<BudgetBucket, decimal>();

            if (filter.Cleared || filter.BeginDate == null)
            {
                return ledgers;
            }

            Dictionary<BudgetBucket, decimal> ledgersSummary = CalculateLedgersBalanceSummary(ledgerBook, filter.BeginDate.Value, statement);

            // Check Surplus
            var surplusBalance = CalculateCurrentMonthSurplusBalance(ledgerBook, filter, statement);
            ledgersSummary.Add(new SurplusBucket(), surplusBalance);

            return ledgersSummary;
        }

        /// <summary>
        ///     Calculates the current month surplus balance.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public virtual decimal CalculateCurrentMonthSurplusBalance([NotNull] LedgerBook ledgerBook,
                                                                   [NotNull] GlobalFilterCriteria filter, [NotNull] StatementModel statement)
        {
            CheckCacheForCleanUp();
            if (ledgerBook == null)
            {
                throw new ArgumentNullException(nameof(ledgerBook));
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (filter.Cleared || filter.BeginDate == null)
            {
                return 0;
            }

            var entryLine = LocateApplicableLedgerLine(ledgerBook, filter);
            if (entryLine == null)
            {
                return 0;
            }

            var beginningOfMonthBalance = entryLine.CalculatedSurplus;
            var transactionTotal = CalculateTransactionTotal(filter.BeginDate.Value, statement, entryLine,
                SurplusBucket.SurplusCode);

            beginningOfMonthBalance += transactionTotal;

            // Find any ledgers that are overpsent and subtract them from the Surplus total.  This is actually what is happening when you overspend a ledger, it spills over and spend Surplus.
            Dictionary<BudgetBucket, decimal> ledgersSummary = CalculateLedgersBalanceSummary(ledgerBook, filter.BeginDate.Value, statement);
            beginningOfMonthBalance += ledgersSummary.Where(kvp => kvp.Value < 0).Sum(kvp => kvp.Value);

            return beginningOfMonthBalance;
        }

        /// <summary>
        ///     Finds any overspent ledgers for the month and returns the date and value the of the total overspend.  This
        ///     resulting
        ///     collection can then be used to subtract from Surplus.
        ///     Overdrawn ledgers are supplemented from Surplus.
        ///     Negative values indicate overdrawn ledgers.
        /// </summary>
        public virtual IEnumerable<ReportTransaction> CalculateOverspentLedgers([NotNull] StatementModel statement,
                                                                                [NotNull] LedgerBook ledger, DateTime beginDate)
        {
            CheckCacheForCleanUp();

            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (ledger == null)
            {
                throw new ArgumentNullException(nameof(ledger));
            }

            // Given the same ledger, statement and begin date this data won't change.
            return (IEnumerable<ReportTransaction>) GetOrAddFromCache(
                BuildCacheKey(statement, ledger, beginDate),
                () =>
                {
                    var overSpendTransactions = new List<ReportTransaction>();
                    var ledgerLine = LocateApplicableLedgerLine(ledger, beginDate);
                    if (ledgerLine == null)
                    {
                        return overSpendTransactions;
                    }

                    var endDate = beginDate.AddMonths(1);
                    var currentDate = beginDate;
                    Dictionary<BudgetBucket, decimal> runningBalances = ledgerLine.Entries.ToDictionary(entry => entry.LedgerBucket.BudgetBucket,
                        entry => entry.Balance);
                    Dictionary<BudgetBucket, decimal> previousBalances = ledgerLine.Entries.ToDictionary(entry => entry.LedgerBucket.BudgetBucket,
                        entry => 0M);

                    do
                    {
                        var currentDateCopy = currentDate;
                        foreach (var transaction in statement.Transactions.Where(t => t.Date == currentDateCopy))
                        {
                            if (runningBalances.ContainsKey(transaction.BudgetBucket))
                            {
                                runningBalances[transaction.BudgetBucket] += transaction.Amount;
                            }
                        }

                        ProcessOverdrawnLedgers(runningBalances, previousBalances, overSpendTransactions, currentDate);

                        currentDate = currentDate.AddDays(1);
                    } while (currentDate < endDate);
                    return overSpendTransactions;
                });
        }

        /// <summary>
        ///     Locates the most recent <see cref="LedgerEntryLine" /> for the given date filter. Note that this will only return
        ///     the most recent line that fits the criteria.
        /// </summary>
        public virtual decimal LocateApplicableLedgerBalance([NotNull] LedgerBook ledgerBook,
                                                             [NotNull] GlobalFilterCriteria filter, string bucketCode)
        {
            CheckCacheForCleanUp();
            if (ledgerBook == null)
            {
                throw new ArgumentNullException(nameof(ledgerBook));
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var line = LocateApplicableLedgerLine(ledgerBook, filter);
            if (line == null)
            {
                return 0;
            }

            return line.Entries
                .Where(ledgerEntry => ledgerEntry.LedgerBucket.BudgetBucket.Code == bucketCode)
                .Select(ledgerEntry => ledgerEntry.Balance)
                .FirstOrDefault();
        }

        /// <summary>
        ///     Locates the applicable ledger line.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public virtual LedgerEntryLine LocateApplicableLedgerLine(LedgerBook ledgerBook,
                                                                  [NotNull] GlobalFilterCriteria filter)
        {
            CheckCacheForCleanUp();
            if (ledgerBook == null)
            {
                return null;
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (filter.Cleared)
            {
                return ledgerBook.Reconciliations.FirstOrDefault();
            }

            Debug.Assert(filter.BeginDate != null);
            Debug.Assert(filter.EndDate != null);
            return LocateLedgerEntryLine(ledgerBook, filter.BeginDate.Value, filter.EndDate.Value);
        }

        /// <summary>
        ///     Locates the applicable ledger line.
        /// </summary>
        public virtual LedgerEntryLine LocateApplicableLedgerLine(LedgerBook ledgerBook, DateTime beginDate)
        {
            CheckCacheForCleanUp();
            if (ledgerBook == null)
            {
                return null;
            }

            return LocateLedgerEntryLine(ledgerBook, beginDate, beginDate.AddMonths(1).AddDays(-1));
        }

        private static string BuildCacheKey(object dependency1, object dependency2, DateTime dependentDate)
        {
            long key;
            unchecked
            {
                key = dependency1?.GetHashCode() ?? 1 * dependency2?.GetHashCode() ?? 1 * dependentDate.GetHashCode();
            }

            var keyString = key.ToString(CultureInfo.InvariantCulture);
            return keyString;
        }

        private Dictionary<BudgetBucket, decimal> CalculateLedgersBalanceSummary(LedgerBook ledgerBook,
                                                                                 DateTime beginDate, StatementModel statement)
        {
            var endDate = beginDate.AddMonths(1).AddDays(-1);
            var currentLegderLine = LocateLedgerEntryLine(ledgerBook, beginDate, endDate);
            if (currentLegderLine == null)
            {
                return new Dictionary<BudgetBucket, decimal>();
            }

            var ledgersSummary = new Dictionary<BudgetBucket, decimal>();
            foreach (var entry in currentLegderLine.Entries)
            {
                var closingBalance = CalculateTransactionTotal(beginDate, statement, currentLegderLine,
                    entry.LedgerBucket.BudgetBucket.Code);
                var balance = entry.Balance + closingBalance;
                ledgersSummary.Add(entry.LedgerBucket.BudgetBucket, balance);
            }

            return ledgersSummary;
        }

        private decimal CalculateTransactionTotal(
            DateTime beginDate,
            [NotNull] StatementModel statement,
            [CanBeNull] LedgerEntryLine entryLine,
            string bucketCode)
        {
            var autoMatchLedgerTransactions = (List<LedgerTransaction>) GetOrAddFromCache(
                BuildCacheKey(statement, entryLine, beginDate),
                () => ReconciliationBuilder.FindAutoMatchingTransactions(entryLine, true).ToList());

            this.logger.LogInfo(
                l =>
                {
                    var builder = new StringBuilder();
                    builder.AppendLine("Ledger Transactions found that are 'Auto-Matching-Transactions':");
                    foreach (var txn in autoMatchLedgerTransactions)
                    {
                        builder.AppendLine(
                            $"{txn.Date:d}   {txn.Amount:F2}  {txn.Narrative}  {txn.AutoMatchingReference}");
                    }
                    return builder.ToString();
                });

            IEnumerable<Transaction> query = statement.Transactions
                .Where(t => t.Date < beginDate.AddMonths(1))
                .Where(txn => !ReconciliationBuilder.IsAutoMatchingTransaction(txn, autoMatchLedgerTransactions));
            if (bucketCode == SurplusBucket.SurplusCode)
            {
                // This is to allow inclusion of special Surplus bucket subclasses. (IE: Special Project Surplus buckets)
                query = query.Where(t => t.BudgetBucket is SurplusBucket);
            }
            else
            {
                query = query.Where(t => t.BudgetBucket != null && t.BudgetBucket.Code == bucketCode);
            }

            this.logger.LogInfo(
                l =>
                {
                    var builder = new StringBuilder();
                    builder.AppendLine(
                        $"Statement Transactions found that are '{bucketCode}' and not 'Auto-Matching-Transactions':");
                    foreach (var txn in query)
                    {
                        builder.AppendLine($"{txn.Date:d}   {txn.Amount:F2}  {txn.Description}  {txn.Account}");
                    }
                    return builder.ToString();
                });
            var transactionTotal = query.Sum(txn => txn.Amount);
            this.logger.LogInfo(l => l.Format("Total Transactions {0:F2}", transactionTotal));
            return transactionTotal;
        }

        private static void CheckCacheForCleanUp()
        {
            var wasLastUsed = DateTime.Now.Subtract(CacheLastUpdated);
            if (wasLastUsed.Minutes > 2 && CacheLastUpdated != default(DateTime))
            {
                CacheLastUpdated = default(DateTime);
                CalculationsCache.Clear();
            }
        }

        private static object GetOrAddFromCache(string cacheKey, Func<object> factory)
        {
            var wrappedFactory = new Func<object>(
                () =>
                {
                    CacheLastUpdated = DateTime.Now;
                    return factory();
                });
            return CalculationsCache.GetOrAdd(cacheKey, key => wrappedFactory());
        }

        private static LedgerEntryLine LocateLedgerEntryLine(LedgerBook ledgerBook, DateTime begin, DateTime end)
        {
            return
                ledgerBook.Reconciliations.FirstOrDefault(
                    ledgerEntryLine => ledgerEntryLine.Date >= begin && ledgerEntryLine.Date <= end);
        }

        private static void ProcessOverdrawnLedgers(
            Dictionary<BudgetBucket, decimal> runningBalances,
            Dictionary<BudgetBucket, decimal> previousBalances,
            List<ReportTransaction> overSpendTransactions,
            DateTime currentDate)
        {
            foreach (KeyValuePair<BudgetBucket, decimal> runningBalance in runningBalances)
            {
                var previousBalance = previousBalances[runningBalance.Key];

                // Update previous balance with today's
                previousBalances[runningBalance.Key] = runningBalance.Value;

                if (previousBalance == runningBalance.Value)
                {
                    // Previous balance and current balance are the same so there is no change, ledger hasn't got any worse or better.
                    continue;
                }

                if (runningBalance.Value < 0 && previousBalance >= 0)
                {
                    // Ledger has been overdrawn today.
                    overSpendTransactions.Add(
                        new ReportTransaction
                        {
                            Date = currentDate,
                            Amount = runningBalance.Value,
                            Narrative = runningBalance.Key + " overdrawn - will be supplemented from Surplus."
                        });
                    continue;
                }

                if (runningBalance.Value < 0 && previousBalance < 0)
                {
                    // Ledger was overdrawn yesterday and is still overdrawn today. Ensure the difference is added to the overSpend.
                    var amount = -(previousBalance - runningBalance.Value);
                    overSpendTransactions.Add(
                        new ReportTransaction
                        {
                            Date = currentDate,
                            Amount = amount,
                            Narrative = string.Format(
                                CultureInfo.CurrentCulture,
                                "{0} was overdrawn, {1}. Will be supplemented from Surplus.",
                                runningBalance.Key,
                                amount < 0
                                    ? "and has been further overdrawn"
                                    : "has been credited, but is still overdrawn")
                        });
                    continue;
                }

                if (runningBalance.Value >= 0 && previousBalance < 0)
                {
                    // Ledger was overdrawn yesterday and is now back in credit.
                    overSpendTransactions.Add(
                        new ReportTransaction
                        {
                            Date = currentDate,
                            Amount = -previousBalance,
                            Narrative =
                                runningBalance.Key +
                                " was overdrawn, and has been credited back into a positive balance."
                        });
                }
            }
        }
    }
}