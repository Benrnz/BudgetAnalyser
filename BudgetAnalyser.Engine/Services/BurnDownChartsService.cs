using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC]
    internal class BurnDownChartsService : IBurnDownChartsService
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly IBurnDownChartAnalyser chartAnalyser;
        private readonly BurnDownChartsBuilder chartsBuilder;

        public BurnDownChartsService([NotNull] IBudgetBucketRepository bucketRepository,
                                     [NotNull] BurnDownChartsBuilder chartsBuilder,
                                     [NotNull] IBurnDownChartAnalyser chartAnalyser)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }

            if (chartsBuilder == null)
            {
                throw new ArgumentNullException(nameof(chartsBuilder));
            }

            if (chartAnalyser == null)
            {
                throw new ArgumentNullException(nameof(chartAnalyser));
            }

            this.bucketRepository = bucketRepository;
            this.chartsBuilder = chartsBuilder;
            this.chartAnalyser = chartAnalyser;
        }

        public IEnumerable<BudgetBucket> AvailableBucketsForBurnDownCharts()
        {
            return this.bucketRepository.Buckets.Where(b => b is ExpenseBucket || b is SurplusBucket);
        }

        public BurnDownCharts BuildAllCharts(
            StatementModel statementModel,
            BudgetModel budgetModel,
            LedgerBook ledgerBookModel, 
            GlobalFilterCriteria criteria)
        {
            if (criteria == null) throw new ArgumentNullException(nameof(criteria));
            if (criteria.Cleared) throw new ArgumentException("There is no date range criteria set. This graph is intended for one month of data.");
            if (criteria.EndDate == null || criteria.BeginDate == null)
            {
                throw new ArgumentException("There is no date range set; either the begin or end date is not set. This graph is intended for one month of data.");
            }
            if (criteria.EndDate.Value.Subtract(criteria.EndDate.Value).Days > 31)
            {
                throw new ArgumentException("The date range is too great for this graph. This graph is intended for one month of data.");
            }
            this.chartsBuilder.Build(criteria, statementModel, budgetModel, ledgerBookModel);
            return this.chartsBuilder.Results;
        }

        public BurnDownChartAnalyserResult CreateNewCustomAggregateChart(
            StatementModel statementModel,
            BudgetModel budgetModel,
            IEnumerable<BudgetBucket> buckets,
            LedgerBook ledgerBookModel,
            DateTime beginDate,
            string chartTitle)
        {
            List<BudgetBucket> bucketsList = buckets.ToList();
            var result = this.chartAnalyser.Analyse(statementModel, budgetModel, bucketsList, ledgerBookModel, beginDate);
            result.ChartTitle = chartTitle;
            var persistChart = new CustomAggregateBurnDownGraph
            {
                BucketIds = bucketsList.Select(b => b.Code).ToList(),
                Name = chartTitle
            };

            this.chartsBuilder.CustomCharts = this.chartsBuilder.CustomCharts.Union(new[] { persistChart }).ToList();
            return result;
        }

        public void LoadPersistedStateData(CustomBurnDownChartApplicationState persistedStateData)
        {
            if (persistedStateData == null)
            {
                throw new ArgumentNullException(nameof(persistedStateData));
            }

            this.chartsBuilder.CustomCharts = persistedStateData.Charts;
        }

        public CustomBurnDownChartApplicationState PreparePersistentStateData()
        {
            IEnumerable<CustomAggregateBurnDownGraph> charts = this.chartsBuilder.CustomCharts ?? new List<CustomAggregateBurnDownGraph>();
            return new CustomBurnDownChartApplicationState
            {
                Charts = charts.ToList()
            };
        }

        public void RemoveCustomChart(string chartName)
        {
            List<CustomAggregateBurnDownGraph> customCharts = this.chartsBuilder.CustomCharts.ToList();
            var chart = customCharts.FirstOrDefault(c => c.Name == chartName);
            customCharts.Remove(chart);
            this.chartsBuilder.CustomCharts = customCharts;
        }
    }
}