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
    public class SpendingGraphAnalyser : ISpendingGraphAnalyser
    {
        /// <summary>
        ///     Gets a collection of x,y cordinate values used to plot a graph line. Using a List of Key Value Pairs is more
        ///     friendly with the graph control than a dictionary.
        ///     These values shows actual spending over the month.
        /// </summary>
        public List<KeyValuePair<DateTime, decimal>> ActualSpending { get; private set; }

        public decimal ActualSpendingAxesMinimum { get; private set; }

        /// <summary>
        ///     Gets a collection of x,y cordinate values used to plot a graph line.
        ///     These values shows how the budget should be spent in a linear fashion over the month.
        ///     Using a List of Key Value Pairs is more friendly with the graph control than a dictionary.
        /// </summary>
        public List<KeyValuePair<DateTime, decimal>> BudgetLine { get; private set; }

        public decimal NetWorth { get; private set; }

        /// <summary>
        ///     Gets a collection of x,y cordinate values used to plot a graph line. Using a List of Key Value Pairs is more
        ///     friendly with the graph control than a dictionary.
        ///     These values are used draw a horizontal zero line on the graph.
        /// </summary>
        public List<KeyValuePair<DateTime, decimal>> ZeroLine { get; private set; }

        public void Analyse(
            StatementModel statementModel,
            BudgetModel budgetModel,
            IEnumerable<BudgetBucket> buckets,
            GlobalFilterCriteria filterCriteria,
            LedgerBook ledgerBook)
        {
            ZeroLine = null;
            BudgetLine = null;
            ActualSpending = null;

            Dictionary<DateTime, decimal> chartData = YieldAllDaysInDateRange(statementModel, filterCriteria);
            var earliestDate = chartData.Keys.Min(k => k);
            var latestDate = chartData.Keys.Max(k => k);
            ZeroLine = chartData.ToList();

            List<BudgetBucket> bucketsCopy = buckets.ToList();

            var query = statementModel.Transactions
                .Join(bucketsCopy, t => t.BudgetBucket, b => b, (t, b) => t)
                .Where(t => t.Date >= filterCriteria.BeginDate && t.Date <= filterCriteria.EndDate)
                .GroupBy(t => t.Date, (date, txns) => new { Date = date, Total = txns.Sum(t => -t.Amount) })
                .OrderBy(t => t.Date)
                .AsParallel();

            foreach (var transaction in query)
            {
                chartData[transaction.Date] = transaction.Total;
            }

            decimal budgetTotal = GetBudgetedTotal(budgetModel, bucketsCopy, statementModel.DurationInMonths, ledgerBook, earliestDate, latestDate);

            decimal runningTotal = budgetTotal;
            ActualSpending = new List<KeyValuePair<DateTime, decimal>>(chartData.Count);
            foreach (var day in chartData)
            {
                NetWorth += day.Value;
                runningTotal -= day.Value;
                ActualSpending.Add(new KeyValuePair<DateTime, decimal>(day.Key, runningTotal));
            }

            CalculateBudgetLineValues(budgetTotal);

            ActualSpendingAxesMinimum = runningTotal < 0 ? runningTotal : 0;
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
                    budgetTotal += bucketsCopy.Sum(bucket => GetLedgerBalance(applicableLine, bucket));
                }
            }

            return budgetTotal;
        }

        private static decimal GetLedgerBalance(LedgerEntryLine applicableLine, BudgetBucket bucket)
        {
            if (applicableLine == null)
            {
                return 0;
            }

            if (bucket is SurplusBucket)
            {
                return applicableLine.CalculatedSurplus;
            }

            LedgerEntry ledger = applicableLine.Entries.FirstOrDefault(e => e.Ledger.BudgetBucket == bucket);
            if (ledger != null)
            {
                return ledger.Balance;
            }

            return 0;
        }

        private static Dictionary<DateTime, decimal> YieldAllDaysInDateRange(StatementModel statementModel, GlobalFilterCriteria criteria)
        {
            DateTime startDate, end;
            if (criteria.BeginDate == null)
            {
                startDate = statementModel.Transactions.Min(t => t.Date);
            }
            else
            {
                startDate = criteria.BeginDate.Value;
            }

            if (criteria.EndDate == null)
            {
                end = statementModel.Transactions.Max(t => t.Date);
            }
            else
            {
                end = criteria.EndDate.Value;
            }

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
            decimal average = budgetTotal / ActualSpending.Count;

            BudgetLine = new List<KeyValuePair<DateTime, decimal>>();
            int iteration = 0;
            foreach (var day in ActualSpending)
            {
                BudgetLine.Add(new KeyValuePair<DateTime, decimal>(day.Key, budgetTotal - (average * iteration++)));
            }
        }
    }
}