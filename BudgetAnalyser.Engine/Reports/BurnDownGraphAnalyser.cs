using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     Calculates spending data for one <see cref="BudgetBucket" /> and compares it to budget model data.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BurnDownGraphAnalyser : IBurnDownGraphAnalyser
    {
        private readonly ILogger logger;

        /// <summary>
        /// A temporary cache with a short timeout to store results from <see cref="CalculateOverspentLedgers"/>. This method is called multiple times over a short period to build a burn down report.
        /// It is IDisposable, but because it is static there is no need to dispose it, it will be disposed when the app exits.
        /// </summary>
        // TODO not happy with this
        private static readonly MemoryCache OverSpentLedgersCache = new MemoryCache("BurnDownGraphAnalyser#OverSpentLedgersCache");
        private List<KeyValuePair<DateTime, decimal>> actualSpending;
        private List<KeyValuePair<DateTime, decimal>> budgetLine;

        public BurnDownGraphAnalyser([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
        }

        /// <summary>
        ///     Gets a collection of x,y cordinate values used to plot a graph line. Using a List of Key Value Pairs is more
        ///     friendly with the graph control than a dictionary.
        ///     These values shows actual spending over the month.
        /// </summary>
        public IEnumerable<KeyValuePair<DateTime, decimal>> ActualSpending
        {
            get { return this.actualSpending; }
        }

        public decimal ActualSpendingAxesMinimum { get; private set; }

        /// <summary>
        ///     Gets a collection of x,y cordinate values used to plot a graph line.
        ///     These values shows how the budget should be spent in a linear fashion over the month.
        ///     Using a List of Key Value Pairs is more friendly with the graph control than a dictionary.
        /// </summary>
        public IEnumerable<KeyValuePair<DateTime, decimal>> BudgetLine
        {
            get { return this.budgetLine; }
        }

        public decimal NetWorth { get; private set; }

        /// <summary>
        ///     Gets a collection of x,y cordinate values used to plot a graph line. Using a List of Key Value Pairs is more
        ///     friendly with the graph control than a dictionary.
        ///     These values are used draw a horizontal zero line on the graph.
        /// </summary>
        public IEnumerable<KeyValuePair<DateTime, decimal>> ZeroLine { get; private set; }

        public void Analyse(
            [NotNull] StatementModel statementModel,
            BudgetModel budgetModel,
            IEnumerable<BudgetBucket> bucketsSubset,
            DateTime beginDate,
            LedgerBook ledgerBook)
        {
            if (statementModel == null)
            {
                throw new ArgumentNullException("statementModel");
            }

            this.logger.LogInfo(() => "BurnDownGraphAnalyser.Analyse: " + string.Join(" ", bucketsSubset.Select(b => b.Code)));
            ZeroLine = null;
            this.budgetLine = null;
            this.actualSpending = null;

            Dictionary<DateTime, decimal> chartData = YieldAllDaysInDateRange(beginDate);
            DateTime earliestDate = beginDate;
            DateTime latestDate = chartData.Keys.Max(k => k);
            ZeroLine = chartData.ToList();
            List<BudgetBucket> bucketsCopy = bucketsSubset.ToList();
            decimal budgetTotal = GetBudgetedTotal(budgetModel, bucketsCopy, statementModel.DurationInMonths, ledgerBook, earliestDate, latestDate);

            var query = statementModel.Transactions
                .Join(bucketsCopy, t => t.BudgetBucket, b => b, (t, b) => t)
                .Where(t => t.Date >= earliestDate && t.Date <= latestDate)
                .GroupBy(t => t.Date, (date, txns) => new { Date = date, Total = txns.Sum(t => -t.Amount) })
                .OrderBy(t => t.Date)
                .AsParallel();

            this.logger.LogInfo(() => "    Initial Daily Txn Totals:");
            foreach (var transaction in query)
            {
                chartData[transaction.Date] = transaction.Total;
                this.logger.LogInfo(() => this.logger.Format("    {0} {1:N}", transaction.Date, transaction.Total));
            }

            if (bucketsCopy.OfType<SurplusBucket>().Any())
            {
                this.logger.LogInfo(() => "    Overspent Ledgers Subtract from Surplus:");
                foreach (var transaction in CalculateOverspentLedgers(statementModel, ledgerBook, beginDate))
                {
                    chartData[transaction.Key] -= transaction.Value;
                    this.logger.LogInfo(() => this.logger.Format("    {0} {1:N}", transaction.Key, transaction.Value));
                }
            }

            decimal runningTotal = budgetTotal;
            this.actualSpending = new List<KeyValuePair<DateTime, decimal>>(chartData.Count);
            this.logger.LogInfo(() => "    Convert totals to running total burndown:");
            foreach (var day in chartData)
            {
                if (day.Key > DateTime.Today)
                {
                    break;
                }

                NetWorth += day.Value;
                runningTotal -= day.Value;
                this.actualSpending.Add(new KeyValuePair<DateTime, decimal>(day.Key, runningTotal));
                this.logger.LogInfo(() => this.logger.Format("    {0} {1:N}", day.Key, runningTotal));
            }

            CalculateBudgetLineValues(budgetTotal);

            ActualSpendingAxesMinimum = runningTotal < 0 ? runningTotal : 0;
        }

        private string BuildCacheKeyForOverSpentLedgers(StatementModel statement, LedgerBook ledger, DateTime beginDate)
        {
            long key;
            unchecked
            {
                key = statement.GetHashCode() * ledger.GetHashCode() * beginDate.GetHashCode();
            }

            string keyString = key.ToString(CultureInfo.InvariantCulture);
            this.logger.LogInfo(() => this.logger.Format("OverSpentLedger Cache Key: {0} from {1} {2} {3}", keyString, statement.FileName, ledger.FileName, beginDate));
            return keyString;
        }

        private static decimal GetBudgetModelTotalForBucket(BudgetModel budgetModel, BudgetBucket bucket)
        {
            if (bucket is JournalBucket)
            {
                // Ignore
                return 0;
            }

            if (bucket is IncomeBudgetBucket)
            {
                throw new InvalidOperationException("Code Error: Income bucket detected when Bucket Spending only applies to Expenses.");
            }

            // Use budget values instead
            if (bucket is SurplusBucket)
            {
                return budgetModel.Surplus;
            }

            Expense budget = budgetModel.Expenses.FirstOrDefault(e => e.Bucket == bucket);
            if (budget != null)
            {
                return budget.Amount;
            }

            return 0;
        }

        private static decimal GetBudgetedTotal(
            [NotNull] BudgetModel budgetModel,
            [NotNull] IEnumerable<BudgetBucket> buckets,
            int durationInMonths,
            LedgerBook ledgerBook,
            DateTime beginDate,
            DateTime endDate)
        {
            decimal budgetTotal = 0;
            List<BudgetBucket> bucketsCopy = buckets.ToList();
            for (int monthIndex = 0; monthIndex < durationInMonths; monthIndex++)
            {
                var previousMonthCriteria = new GlobalFilterCriteria
                {
                    BeginDate = beginDate.AddMonths(-monthIndex),
                    EndDate = endDate.AddMonths(-monthIndex),
                };

                LedgerEntryLine applicableLine = LedgerCalculation.LocateApplicableLedgerLine(ledgerBook, previousMonthCriteria);
                if (applicableLine == null)
                {
                    // Use budget values from budget model instead, there is no ledger book line for this month.
                    budgetTotal += bucketsCopy.Sum(bucket => GetBudgetModelTotalForBucket(budgetModel, bucket));
                }
                else
                {
                    budgetTotal += bucketsCopy.Sum(bucket =>
                    {
                        decimal ledgerBal = GetLedgerBalance(applicableLine, bucket);
                        if (ledgerBal < 0)
                        {
                            // The Ledger line might not actually have a ledger for the given bucket.
                            return GetBudgetModelTotalForBucket(budgetModel, bucket);
                        }

                        return ledgerBal;
                    });
                }
            }

            return budgetTotal;
        }

        private static decimal GetLedgerBalance(LedgerEntryLine applicableLine, BudgetBucket bucket)
        {
            if (applicableLine == null)
            {
                return -1;
            }

            if (bucket is SurplusBucket)
            {
                return applicableLine.CalculatedSurplus;
            }

            LedgerEntry ledger = applicableLine.Entries.FirstOrDefault(e => e.LedgerColumn.BudgetBucket == bucket);
            if (ledger != null)
            {
                return ledger.Balance;
            }

            return -1;
        }

        private static Dictionary<DateTime, decimal> YieldAllDaysInDateRange(DateTime beginDate)
        {
            DateTime startDate = beginDate;
            DateTime end = beginDate.AddMonths(1).AddDays(-1);

            var data = new Dictionary<DateTime, decimal>();
            DateTime current = startDate;
            do
            {
                data.Add(current, 0);
                current = current.AddDays(1);
            } while (current <= end);

            return data;
        }

        private void AddToOverSpentLedgerCache(StatementModel statement, LedgerBook ledger, DateTime beginDate, object cacheData)
        {
            string keyString = BuildCacheKeyForOverSpentLedgers(statement, ledger, beginDate);
            OverSpentLedgersCache.Set(keyString, cacheData, DateTime.Now.AddMinutes(3));
            this.logger.LogInfo(() => this.logger.Format("Added {0} to cache, cache now has {1} items.", keyString, OverSpentLedgersCache.GetCount()));
        }

        private void CalculateBudgetLineValues(decimal budgetTotal)
        {
            decimal average = budgetTotal / ZeroLine.Count();

            this.budgetLine = new List<KeyValuePair<DateTime, decimal>>();
            int iteration = 0;
            foreach (var day in ZeroLine)
            {
                this.budgetLine.Add(new KeyValuePair<DateTime, decimal>(day.Key, budgetTotal - (average * iteration++)));
            }
        }

        /// <summary>
        ///     Finds any overspent ledgers for the month and returns the date and value the of the overspend.  This resulting
        ///     collection can then be used to subtract from Surplus.
        ///     Overdrawn ledgers are supplemented from Surplus.
        ///     Negative values indicate overdrawn ledgers.
        /// </summary>
        private IEnumerable<KeyValuePair<DateTime, decimal>> CalculateOverspentLedgers(StatementModel statement, LedgerBook ledger, DateTime beginDate)
        {
            // TODO this should be moved to LedgerCalculator
            // Given the same ledger, statement and begin date this data won't change.
            IEnumerable<KeyValuePair<DateTime, decimal>> cachedValue = FoundInOverSpentLedgerCache(statement, ledger, beginDate);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var list = new Dictionary<DateTime, decimal>();
            LedgerEntryLine ledgerLine = LedgerCalculation.LocateApplicableLedgerLine(ledger, beginDate);
            if (ledgerLine == null)
            {
                return list;
            }

            DateTime endDate = beginDate.AddMonths(1);
            DateTime currentDate = beginDate;
            Dictionary<BudgetBucket, decimal> runningBalances = ledgerLine.Entries.ToDictionary(entry => entry.LedgerColumn.BudgetBucket, entry => entry.Balance);
            Dictionary<BudgetBucket, decimal> previousBalances = ledgerLine.Entries.ToDictionary(entry => entry.LedgerColumn.BudgetBucket, entry => 0M);

            do
            {
                DateTime currentDateCopy = currentDate;
                foreach (Transaction transaction in statement.Transactions.Where(t => t.Date == currentDateCopy))
                {
                    if (runningBalances.ContainsKey(transaction.BudgetBucket))
                    {
                        runningBalances[transaction.BudgetBucket] += transaction.Amount;
                    }
                }

                decimal overSpend = 0; // This will be a negative number to give the level of overspend.
                foreach (var runningBalance in runningBalances)
                {
                    decimal previousBalance = previousBalances[runningBalance.Key];

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
                        overSpend += runningBalance.Value;
                    }
                    else if (runningBalance.Value < 0 && previousBalance < 0)
                    {
                        // Ledger was overdrawn yesterday and is still overdrawn today. Ensure the difference is added to the overSpend.
                        overSpend += -(previousBalance - runningBalance.Value);
                    }
                    else if (runningBalance.Value >= 0 && previousBalance < 0)
                    {
                        // Ledger was overdrawn yesterday and is now back in credit.
                        overSpend += -previousBalance;
                    }
                }

                list[currentDate] = overSpend;
                currentDate = currentDate.AddDays(1);
            } while (currentDate < endDate);

            AddToOverSpentLedgerCache(statement, ledger, beginDate, list);
            return list;
        }

        private IEnumerable<KeyValuePair<DateTime, decimal>> FoundInOverSpentLedgerCache(StatementModel statement, LedgerBook ledger, DateTime beginDate)
        {
            string keyString = BuildCacheKeyForOverSpentLedgers(statement, ledger, beginDate);
            var item = OverSpentLedgersCache.Get(keyString);
            if (item != null)
            {
                this.logger.LogInfo(() => "Cached value hit");
                return (IEnumerable<KeyValuePair<DateTime, decimal>>)item;
            }

            this.logger.LogInfo(() => "Not found in cache");
            return null;
        }
    }
}