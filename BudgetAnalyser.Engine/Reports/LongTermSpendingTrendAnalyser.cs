using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     This class will analyse a <see cref="StatementModel" /> and create a month based graph per
    ///     <see cref="BudgetBucket" />.
    /// </summary>
    public class LongTermSpendingTrendAnalyser
    {
        private readonly IBudgetBucketRepository budgetBucketRepo;

        public LongTermSpendingTrendAnalyser([NotNull] IBudgetBucketRepository budgetBucketRepo)
        {
            if (budgetBucketRepo == null)
            {
                throw new ArgumentNullException("budgetBucketRepo");
            }

            this.budgetBucketRepo = budgetBucketRepo;
        }

        public GraphData Graph { get; private set; }

        public void Analyse([NotNull] StatementModel statement, [NotNull] GlobalFilterCriteria criteria)
        {
            if (statement == null)
            {
                throw new ArgumentNullException("statement");
            }
            
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            Graph = null;
            DateTime minDate = CalculateStartDate(statement, criteria);
            DateTime maxDate = CalculateEndDate(statement, criteria);

            DateTime currentMonth = minDate;
            DateTime nextMonth = currentMonth.AddMonths(1);
            var subTotals = new Dictionary<string, decimal>();
            List<SeriesData> allSeriesData = InitialiseSeriesData(minDate, maxDate);

            foreach (Transaction transaction in statement.AllTransactions)
            {
                if (transaction.Date >= nextMonth)
                {
                    // Current month's data is complete - update totals and advance to next month
                    foreach (var subTotal in subTotals)
                    {
                        // Find appropriate bucket series
                        SeriesData series = allSeriesData.Single(a => a.SeriesName == subTotal.Key);
                        // Find appropriate month on that bucket graph line
                        DatedGraphPlot monthData = series.PlotsList.Single(s => s.Date == currentMonth);
                        monthData.Amount = Math.Abs(subTotal.Value); // Negate because all debits are stored as negative. Graph lines will look better as positive values.
                    }

                    currentMonth = nextMonth;
                    nextMonth = nextMonth.AddMonths(1);
                    subTotals = new Dictionary<string, decimal>();
                    if (currentMonth >= maxDate)
                    {
                        break;
                    }
                }

                GetOrAdd(subTotals, transaction.BudgetBucket.Code, transaction.Amount);
            }

            Graph = new GraphData { SeriesList = allSeriesData, GraphName = "Long term spending for Budget Buckets" };
            RemoveSeriesWithNoData();
        }

        private static DateTime CalculateStartDate(StatementModel statement, GlobalFilterCriteria criteria)
        {
            if (criteria.Cleared || criteria.BeginDate == null)
            {
                DateTime minDate = statement.AllTransactions.Min(t => t.Date).Date;
                minDate = minDate.FirstDateInMonth();
                return minDate;
            }

            return criteria.BeginDate.Value;
        }

        private static DateTime CalculateEndDate(StatementModel statement, GlobalFilterCriteria criteria)
        {
            if (criteria.Cleared || criteria.EndDate == null)
            {
                var maxDate = statement.AllTransactions.Max(t => t.Date).Date;
                return maxDate.LastDateInMonth();
            }

            return criteria.EndDate.Value;
        }

        private static void GetOrAdd(IDictionary<string, decimal> dictionary, string key, decimal value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] += value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        private List<SeriesData> InitialiseSeriesData(DateTime minDate, DateTime maxDate)
        {
            List<SeriesData> allSeriesData = this.budgetBucketRepo.Buckets
                .Select(bucket => new SeriesData { SeriesName = bucket.Code, Description = bucket.Description })
                .ToList();

            DateTime currentMonth = minDate;
            do
            {
                foreach (SeriesData seriesData in allSeriesData)
                {
                    seriesData.PlotsList.Add(new DatedGraphPlot { Date = currentMonth, Amount = 0M });
                }

                currentMonth = currentMonth.AddMonths(1);
            } while (currentMonth < maxDate);

            return allSeriesData;
        }

        private void RemoveSeriesWithNoData()
        {
            List<SeriesData> zeroSeries = Graph.SeriesList.Where(s => s.PlotsList.Sum(p => p.Amount) == 0).ToList();
            foreach (SeriesData removeMe in zeroSeries)
            {
                Graph.SeriesList.Remove(removeMe);
            }
        }
    }
}