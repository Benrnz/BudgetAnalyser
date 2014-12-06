using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC]
    public class BurnDownChartsService : IBurnDownChartsService
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly BurnDownChartsBuilder chartsBuilder;
        private readonly IBurnDownChartAnalyser chartAnalyser;

        public BurnDownChartsService([NotNull] IBudgetBucketRepository bucketRepository, [NotNull] BurnDownChartsBuilder chartsBuilder, [NotNull] IBurnDownChartAnalyser chartAnalyser)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            if (chartsBuilder == null)
            {
                throw new ArgumentNullException("chartsBuilder");
            }

            if (chartAnalyser == null)
            {
                throw new ArgumentNullException("chartAnalyser");
            }

            this.bucketRepository = bucketRepository;
            this.chartsBuilder = chartsBuilder;
            this.chartAnalyser = chartAnalyser;
        }

        public IEnumerable<BudgetBucket> AvailableBucketsForBurnDownCharts()
        {
            return this.bucketRepository.Buckets.Where(b => b is ExpenseBucket || b is SurplusBucket);
        }

        public BurnDownCharts BuildCharts(
            GlobalFilterCriteria criteria, 
            StatementModel statementModel, 
            BudgetModel budgetModel, 
            LedgerBook ledgerBookModel)
        {
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
            var bucketsList = buckets.ToList();
            var result = this.chartAnalyser.Analyse(statementModel, budgetModel, bucketsList, ledgerBookModel, beginDate);
            result.ChartTitle = chartTitle;
            var persistChart = new CustomAggregateBurnDownGraph
            {
                BucketIds = bucketsList.Select(b => b.Code).ToList(),
                Name = chartTitle,
            };

            this.chartsBuilder.CustomCharts = this.chartsBuilder.CustomCharts.Union(new[] { persistChart }).ToList();
            return result;
        }

        public void LoadPersistedStateData(object persistedStateData)
        {
            var stateData = (List<CustomAggregateBurnDownGraph>)persistedStateData;
            this.chartsBuilder.CustomCharts = stateData;
        }

        public object PreparePersistentStateData()
        {
            return this.chartsBuilder.CustomCharts.ToList();
        }

        public void RemoveCustomChart(string chartName)
        {
            List<CustomAggregateBurnDownGraph> customCharts = this.chartsBuilder.CustomCharts.ToList();
            CustomAggregateBurnDownGraph chart = customCharts.FirstOrDefault(c => c.Name == chartName);
            customCharts.Remove(chart);
            this.chartsBuilder.CustomCharts = customCharts;
        }
    }
}