using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.OverallPerformance
{
    public class OverallPerformanceController : ControllerBase
    {
        private readonly IOverallPerformanceChartService chartService;
        private bool doNotUseExpenseFilter;
        private bool doNotUseIncomeFilter;

        public OverallPerformanceController([NotNull] IMessenger messenger, [NotNull] IOverallPerformanceChartService chartService) : base(messenger)
        {
            if (chartService is null)
            {
                throw new ArgumentNullException(nameof(chartService));
            }

            this.chartService = chartService;
            this.doNotUseExpenseFilter = true;
        }

        public OverallPerformanceBudgetResult Analysis { get; private set; }

        public bool ExpenseFilter
        {
            [UsedImplicitly] get { return this.doNotUseExpenseFilter; }

            set
            {
                this.doNotUseExpenseFilter = value;
                OnPropertyChanged();
                RefreshCollection();
            }
        }

        public bool IncomeFilter
        {
            get { return this.doNotUseIncomeFilter; }

            set
            {
                this.doNotUseIncomeFilter = value;
                OnPropertyChanged();
                RefreshCollection();
            }
        }

        public double OverallPerformance { [UsedImplicitly] get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required by data binding")]
        [UsedImplicitly]
        public string Title => "Overall Budget Performance";

        public void Load(StatementModel statementModel, BudgetCollection budgets, GlobalFilterCriteria criteria)
        {
            Analysis = this.chartService.BuildChart(statementModel, budgets, criteria);
            OverallPerformance = (double)Analysis.OverallPerformance;
            ExpenseFilter = true;
            IncomeFilter = false;

            OnPropertyChanged(nameof(Analysis));
            ICollectionView view = CollectionViewSource.GetDefaultView(Analysis.Analyses);
            view.Filter = x =>
            {
                var bucketAnalysis = x as BucketPerformanceResult;
                if (bucketAnalysis is null)
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
            if (Analysis?.Analyses is null || Analysis.Analyses.None())
            {
                return;
            }

            CollectionViewSource.GetDefaultView(Analysis.Analyses).Refresh();
        }
    }
}