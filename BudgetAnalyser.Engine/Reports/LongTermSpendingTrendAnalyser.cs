using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     This class will analyse a <see cref="StatementModel" /> and create a month based graph per
    ///     <see cref="BudgetBucket" />.
    /// </summary>
    [AutoRegisterWithIoC]
    internal class LongTermSpendingTrendAnalyser
    {
        private readonly IBudgetBucketRepository budgetBucketRepo;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LongTermSpendingTrendAnalyser" /> class.
        /// </summary>
        /// <param name="budgetBucketRepo">The budget bucket repo.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public LongTermSpendingTrendAnalyser([NotNull] IBudgetBucketRepository budgetBucketRepo)
        {
            if (budgetBucketRepo == null)
            {
                throw new ArgumentNullException(nameof(budgetBucketRepo));
            }

            this.budgetBucketRepo = budgetBucketRepo;
        }

        /// <summary>
        ///     Gets the graph data.
        /// </summary>
        public GraphData Graph { get; private set; }

        /// <summary>
        ///     Analyses the specified statement.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <param name="criteria">The criteria.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public void Analyse([NotNull] StatementModel statement, [NotNull] GlobalFilterCriteria criteria)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            Reset();
            var minDate = CalculateStartDate(statement, criteria);
            var maxDate = CalculateEndDate(statement, criteria);

            var currentMonth = minDate;
            var nextMonth = currentMonth.AddMonths(1);
            var subTotals = new Dictionary<string, decimal>();
            List<SeriesData> allSeriesData = InitialiseSeriesData(minDate, maxDate);

            foreach (var transaction in statement.AllTransactions)
            {
                if (transaction.Date >= nextMonth)
                {
                    StoreSummarisedMonthData(subTotals, allSeriesData, currentMonth);

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

            StoreSummarisedMonthData(subTotals, allSeriesData, currentMonth);
            Graph = new GraphData { SeriesList = allSeriesData, GraphName = "Long term spending for Budget Buckets" };
            RemoveSeriesWithNoData();
        }

        public void Reset()
        {
            Graph = null;
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

        private static DateTime CalculateStartDate(StatementModel statement, GlobalFilterCriteria criteria)
        {
            if (criteria.Cleared || criteria.BeginDate == null)
            {
                var minDate = statement.AllTransactions.Min(t => t.Date).Date;
                minDate = minDate.FirstDateInMonth();
                return minDate;
            }

            return criteria.BeginDate.Value;
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

            var currentMonth = minDate;
            do
            {
                foreach (var seriesData in allSeriesData)
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
            foreach (var removeMe in zeroSeries)
            {
                Graph.SeriesList.Remove(removeMe);
            }
        }

        private static void StoreSummarisedMonthData(Dictionary<string, decimal> subTotals,
                                                     List<SeriesData> allSeriesData, DateTime currentMonth)
        {
            // Current month's data is complete - update totals and advance to next month
            foreach (KeyValuePair<string, decimal> subTotal in subTotals)
            {
                // Find appropriate bucket series
                var series = allSeriesData.Single(a => a.SeriesName == subTotal.Key);
                // Find appropriate month on that bucket graph line
                var monthData = series.PlotsList.Single(s => s.Date == currentMonth);
                monthData.Amount = Math.Abs(subTotal.Value);
                // Negate because all debits are stored as negative. Graph lines will look better as positive values.
            }
        }
    }
}