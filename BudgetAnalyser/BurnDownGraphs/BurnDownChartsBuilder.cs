using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.BurnDownGraphs
{
    public class BurnDownChartsBuilder
    {
        private readonly Func<BucketBurnDownController> bucketSpendingFactory;
        private readonly IBudgetBucketRepository budgetBucketRepository;
        public IEnumerable<CustomAggregateBurnDownGraph> CustomCharts { get; set; }

        public BurnDownChartsBuilder(
            IBudgetBucketRepository budgetBucketRepository,
            Func<BucketBurnDownController> bucketSpendingFactory)
        {
            this.budgetBucketRepository = budgetBucketRepository;
            this.bucketSpendingFactory = bucketSpendingFactory;
        }

        public BurnDownChartBuilderResults Results { get; private set; }

        public void Build(GlobalFilterCriteria criteria,
            StatementModel statementModel,
            BudgetModel budgetModel,
            Engine.Ledger.LedgerBook ledgerBookModel)
        {
            var beginDate = CalculateBeginDate(criteria);
            var dateRangeDescription = String.Format(CultureInfo.CurrentCulture, "For the month starting {0:D} to {1:D} inclusive.", beginDate, beginDate.AddMonths(1).AddDays(-1));

            var listOfCharts = new List<BucketBurnDownController>(this.budgetBucketRepository.Buckets.Count());

            foreach (BudgetBucket bucket in this.budgetBucketRepository.Buckets
                .Where(b => b is ExpenseBucket)
                .OrderBy(b => b.Code))
            {
                BucketBurnDownController chartController = this.bucketSpendingFactory();
                chartController.Load(statementModel, budgetModel, bucket, beginDate, ledgerBookModel);
                listOfCharts.Add(chartController);
            }

            listOfCharts = listOfCharts.OrderBy(x => x.Bucket).ToList();

            // Put surplus at the top.
            listOfCharts.Insert(
                0,
                this.bucketSpendingFactory().Load(statementModel, budgetModel, this.budgetBucketRepository.SurplusBucket, beginDate, ledgerBookModel));

            // Put any custom charts on top.
            foreach (CustomAggregateBurnDownGraph customChart in this.CustomCharts)
            {
                BucketBurnDownController chartController = this.bucketSpendingFactory();
                IEnumerable<BudgetBucket> buckets = this.budgetBucketRepository.Buckets
                    .Join(customChart.BucketIds, bucket => bucket.Code, code => code, (bucket, code) => bucket);
                chartController.LoadCustomChart(statementModel, budgetModel, buckets, beginDate, ledgerBookModel, customChart.Name);
                listOfCharts.Insert(0, chartController);
            }

            Results = new BurnDownChartBuilderResults(beginDate, dateRangeDescription, listOfCharts);
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
    }
}