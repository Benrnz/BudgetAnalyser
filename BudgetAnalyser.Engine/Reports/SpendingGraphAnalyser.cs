using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
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

            decimal budgetTotal = GetBudgetTotal(budgetModel, bucketsCopy, statementModel.DurationInMonths, ledgerBook != null);
            budgetTotal += AddLedgerBalance(ledgerBook, filterCriteria, bucketsCopy);
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

        private static decimal AddLedgerBalance(LedgerBook ledgerBook, GlobalFilterCriteria filterCriteria, IEnumerable<BudgetBucket> buckets)
        {
            if (ledgerBook == null || filterCriteria == null)
            {
                return 0;
            }

            LedgerEntryLine applicableLine = LedgerCalculation.LocateApplicableLedgerLine(ledgerBook, filterCriteria);

            if (applicableLine == null)
            {
                return 0;
            }

            decimal ledgerBalances = 0;
            foreach (var budgetBucket in buckets)
            {
                if (budgetBucket is SurplusBucket)
                {
                    ledgerBalances += applicableLine.CalculatedSurplus;
                }
                else
                {
                    var ledger = applicableLine.Entries.FirstOrDefault(e => e.Ledger.BudgetBucket == budgetBucket);
                    if (ledger != null)
                    {
                        ledgerBalances += ledger.Balance;
                    }
                }
            }

            return ledgerBalances;
        }

        private static decimal GetBudgetTotal(
            BudgetModel budgetModel, 
            IEnumerable<BudgetBucket> buckets, 
            int durationInMonths,
            bool haveLedgerBook)
        {
            decimal budgetTotal = 0;
            foreach (BudgetBucket bucket in buckets)
            {
                if (bucket is SurplusBucket)
                {
                    if (!haveLedgerBook)
                    {
                        budgetTotal += budgetModel.Surplus;
                    }
                }
                else if (bucket is JournalBucket)
                {
                    // Ignore
                }
                else if (bucket is IncomeBudgetBucket)
                {
                    throw new InvalidOperationException("Code Error: Income bucket detected when Bucket Spending only applies to Expenses.");
                }
                else
                {
                    var budget = budgetModel.Expenses.FirstOrDefault(e => e.Bucket == bucket);
                    if (budget != null)
                    {
                        budgetTotal += budget.Amount;
                    }
                }
            }

            return budgetTotal*durationInMonths;
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

            DateTime maxDate = statementModel.Transactions.Max(t => t.Date);
            if (criteria.EndDate == null)
            {
                end = maxDate;
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
            decimal average = budgetTotal/ActualSpending.Count;

            BudgetLine = new List<KeyValuePair<DateTime, decimal>>();
            int iteration = 0;
            foreach (var day in ActualSpending)
            {
                BudgetLine.Add(new KeyValuePair<DateTime, decimal>(day.Key, budgetTotal - (average*iteration++)));
            }
        }
    }
}