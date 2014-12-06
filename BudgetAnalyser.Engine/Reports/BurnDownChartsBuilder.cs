using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports
{
    public class BurnDownChartsBuilder
    {
        private readonly IBudgetBucketRepository budgetBucketRepository;
        private readonly Func<IBurnDownChartAnalyser> chartAnalyserFactory;

        public BurnDownChartsBuilder([NotNull] IBudgetBucketRepository budgetBucketRepository, [NotNull] Func<IBurnDownChartAnalyser> chartAnalyserFactory)
        {
            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            if (chartAnalyserFactory == null)
            {
                throw new ArgumentNullException("chartAnalyserFactory");
            }

            this.budgetBucketRepository = budgetBucketRepository;
            this.chartAnalyserFactory = chartAnalyserFactory;
        }

        public IEnumerable<CustomAggregateBurnDownGraph> CustomCharts { get; set; }

        public BurnDownCharts Results { get; private set; }

        public void Build(GlobalFilterCriteria criteria,
            StatementModel statementModel,
            BudgetModel budgetModel,
            LedgerBook ledgerBookModel)
        {
            DateTime beginDate = CalculateBeginDate(criteria);
            string dateRangeDescription = string.Format(CultureInfo.CurrentCulture, "For the month starting {0:D} to {1:D} inclusive.", beginDate, beginDate.AddMonths(1).AddDays(-1));

            var listOfCharts = new List<BurnDownChartAnalyserResult>(this.budgetBucketRepository.Buckets.Count());
            foreach (BudgetBucket bucket in this.budgetBucketRepository.Buckets
                .Where(b => b is ExpenseBucket)
                .OrderBy(b => b.Code))
            {
                BurnDownChartAnalyserResult analysis = AnalyseDataForChart(statementModel, budgetModel, ledgerBookModel, bucket, beginDate);
                analysis.ChartTitle = string.Format(CultureInfo.CurrentCulture, "{0} Spending Chart", bucket.Code);
                listOfCharts.Add(analysis);
            }

            listOfCharts = listOfCharts.ToList();

            // Put surplus at the top.
            BurnDownChartAnalyserResult analysisResult = AnalyseDataForChart(statementModel, budgetModel, ledgerBookModel, this.budgetBucketRepository.SurplusBucket, beginDate);
            analysisResult.ChartTitle = string.Format(CultureInfo.CurrentCulture, "{0} Spending Chart", this.budgetBucketRepository.SurplusBucket);
            listOfCharts.Insert(0, analysisResult);

            // Put any custom charts on top.
            foreach (CustomAggregateBurnDownGraph customChart in CustomCharts)
            {
                IEnumerable<BudgetBucket> buckets = this.budgetBucketRepository.Buckets
                    .Join(customChart.BucketIds, bucket => bucket.Code, code => code, (bucket, code) => bucket);

                BurnDownChartAnalyserResult analysis = AnalyseDataForChart(statementModel, budgetModel, ledgerBookModel, buckets, beginDate);
                analysis.ChartTitle = customChart.Name;
                analysis.IsCustomAggregateChart = true;
                listOfCharts.Insert(0, analysis);
            }

            Results = new BurnDownCharts(beginDate, dateRangeDescription, listOfCharts);
        }

        private static DateTime CalculateBeginDate(GlobalFilterCriteria criteria)
        {
            if (criteria.Cleared)
            {
                return DateTime.Today.AddMonths(-1);
            }

            if (criteria.BeginDate != null)
            {
                return criteria.BeginDate.Value;
            }

            if (criteria.EndDate == null)
            {
                return DateTime.Today.AddMonths(-1);
            }

            return criteria.EndDate.Value.AddMonths(-1);
        }

        private BurnDownChartAnalyserResult AnalyseDataForChart(StatementModel statementModel, BudgetModel budgetModel, LedgerBook ledgerBookModel, BudgetBucket bucket, DateTime beginDate)
        {
            IBurnDownChartAnalyser analyser = this.chartAnalyserFactory();
            BurnDownChartAnalyserResult result = analyser.Analyse(statementModel, budgetModel, new[] { bucket }, ledgerBookModel, beginDate);
            return result;
        }

        private BurnDownChartAnalyserResult AnalyseDataForChart(StatementModel statementModel, BudgetModel budgetModel, LedgerBook ledgerBookModel, IEnumerable<BudgetBucket> buckets,
            DateTime beginDate)
        {
            IBurnDownChartAnalyser analyser = this.chartAnalyserFactory();
            BurnDownChartAnalyserResult result = analyser.Analyse(statementModel, budgetModel, buckets, ledgerBookModel, beginDate);
            return result;
        }
    }
}