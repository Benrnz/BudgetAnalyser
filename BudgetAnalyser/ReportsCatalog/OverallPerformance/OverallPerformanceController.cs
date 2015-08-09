using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.OverallPerformance
{
    public class OverallPerformanceController : ControllerBase
    {
        private readonly IOverallPerformanceChartService chartService;
        private bool doNotUseExpenseFilter;
        private bool doNotUseIncomeFilter;

        public OverallPerformanceController([NotNull] IOverallPerformanceChartService chartService)
        {
            if (chartService == null)
            {
                throw new ArgumentNullException("chartService");
            }

            this.chartService = chartService;
            this.doNotUseExpenseFilter = true;
        }

        public OverallPerformanceBudgetResult Analysis { get; private set; }

        public bool ExpenseFilter
        {
            get { return this.doNotUseExpenseFilter; }

            set
            {
                this.doNotUseExpenseFilter = value;
                RaisePropertyChanged();
                RefreshCollection();
            }
        }

        public bool IncomeFilter
        {
            get { return this.doNotUseIncomeFilter; }

            set
            {
                this.doNotUseIncomeFilter = value;
                RaisePropertyChanged();
                RefreshCollection();
            }
        }

        public double OverallPerformance { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required by data binding")]
        public string Title
        {
            get { return "Overall Budget Performance"; }
        }

        public void Load(StatementModel statementModel, BudgetCollection budgets, GlobalFilterCriteria criteria)
        {
            Analysis = this.chartService.BuildChart(statementModel, budgets, criteria);
            OverallPerformance = (double)Analysis.OverallPerformance;
            ExpenseFilter = true;
            IncomeFilter = false;

            RaisePropertyChanged(() => Analysis);
            ICollectionView view = CollectionViewSource.GetDefaultView(Analysis.Analyses);
            view.Filter = x =>
            {
                var bucketAnalysis = x as BucketPerformanceResult;
                if (bucketAnalysis == null)
                {
                    return true;
                }

                if (IncomeFilter)
                {
                    return bucketAnalysis.Bucket is IncomeBudgetBucket;
                }

                bool result = !(bucketAnalysis.Bucket is IncomeBudgetBucket);
                return result;
            };
        }

        private void RefreshCollection()
        {
            if (Analysis == null || Analysis.Analyses == null || Analysis.Analyses.None())
            {
                return;
            }

            CollectionViewSource.GetDefaultView(Analysis.Analyses).Refresh();
        }
    }
}