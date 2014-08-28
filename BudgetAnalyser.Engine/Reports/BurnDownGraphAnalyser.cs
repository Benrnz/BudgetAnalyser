using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly LedgerCalculation ledgerCalculator;
        private readonly ILogger logger;

        private List<KeyValuePair<DateTime, decimal>> actualSpending;
        private List<KeyValuePair<DateTime, decimal>> budgetLine;

        public BurnDownGraphAnalyser([NotNull] LedgerCalculation ledgerCalculator, [NotNull] ILogger logger)
        {
            if (ledgerCalculator == null)
            {
                throw new ArgumentNullException("ledgerCalculator");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.ledgerCalculator = ledgerCalculator;
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

        /// <summary>
        /// Analyse the actual spending over a month as a burn down of the available budget.
        /// The available budget is either the <paramref name="ledgerBook"/> balance of the ledger or if not tracked in the ledger, then 
        /// the budgeted amount from the <paramref name="budgetModel"/>.
        /// </summary>
        public void Analyse(
            [NotNull] StatementModel statementModel,
            [NotNull] BudgetModel budgetModel,
            IEnumerable<BudgetBucket> bucketsSubset,
            DateTime beginDate,
            [CanBeNull] LedgerBook ledgerBook)
        {
            if (statementModel == null)
            {
                throw new ArgumentNullException("statementModel");
            }

            if (budgetModel == null)
            {
                throw new ArgumentNullException("budgetModel");
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
            decimal budgetTotal = GetBudgetedTotal(budgetModel, ledgerBook, bucketsCopy, earliestDate);

            CollateAndInsertStatementTransactions(statementModel, bucketsCopy, earliestDate, latestDate, chartData);

            // Only relevant when calculating surplus burndown
            if (ledgerBook != null && bucketsCopy.OfType<SurplusBucket>().Any())
            {
                this.logger.LogInfo(() => "    Overspent Ledgers Subtract from Surplus:");
                foreach (var transaction in this.ledgerCalculator.CalculateOverspentLedgers(statementModel, ledgerBook, beginDate))
                {
                    chartData[transaction.Date] -= transaction.Amount;
                    this.logger.LogInfo(() => this.logger.Format("    {0} {1:N}", transaction.Date, transaction.Amount));
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

        private static void CollateAndInsertStatementTransactions(StatementModel statementModel, IEnumerable<BudgetBucket> bucketsCopy, DateTime earliestDate, DateTime latestDate, Dictionary<DateTime, decimal> chartData)
        {
            var query = statementModel.Transactions
                .Join(bucketsCopy, t => t.BudgetBucket, b => b, (t, b) => t)
                .Where(t => t.Date >= earliestDate && t.Date <= latestDate)
                .GroupBy(t => t.Date, (date, txns) => new { Date = date, Total = txns.Sum(t => -t.Amount) })
                .OrderBy(t => t.Date)
                .AsParallel();

            foreach (var transaction in query)
            {
                chartData[transaction.Date] = transaction.Total;
            }
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

        /// <summary>
        /// Calculates the appropriate budgeted amount for the given buckets.
        /// This can either be the ledger balance from the ledger book or if not tracked by the ledger book, then from the budget model.
        /// </summary>
        private decimal GetBudgetedTotal(
            [NotNull] BudgetModel budgetModel,
            [CanBeNull] LedgerBook ledgerBook,
            [NotNull] IEnumerable<BudgetBucket> buckets,
            DateTime beginDate)
        {
            decimal budgetTotal = 0;
            List<BudgetBucket> bucketsCopy = buckets.ToList();

            LedgerEntryLine applicableLine = this.ledgerCalculator.LocateApplicableLedgerLine(ledgerBook, beginDate);
            if (applicableLine == null)
            {
                // Use budget values from budget model instead, there is no ledger book line for this month.
                budgetTotal += bucketsCopy.Sum(bucket => GetBudgetModelTotalForBucket(budgetModel, bucket));
            }
            else
            {
                budgetTotal += bucketsCopy.Sum(bucket => GetLedgerBalanceForBucket(budgetModel, applicableLine, bucket));
            }

            return budgetTotal;
        }

        private static decimal GetLedgerBalanceForBucket(BudgetModel budgetModel, LedgerEntryLine applicableLine, BudgetBucket bucket)
        {
            if (bucket is SurplusBucket)
            {
                return applicableLine.CalculatedSurplus;
            }

            LedgerEntry ledger = applicableLine.Entries.FirstOrDefault(e => e.LedgerColumn.BudgetBucket == bucket);
            if (ledger != null)
            {
                return ledger.Balance;
            }

            // The Ledger line might not actually have a ledger for the given bucket.
            return GetBudgetModelTotalForBucket(budgetModel, bucket);
        }

        /// <summary>
        /// Populate a dictionary with an entry for each day of a month beginning at the start date.
        /// </summary>
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
    }
}