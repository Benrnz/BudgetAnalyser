using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.OverallPerformance
{
    public class OverallPerformanceController : ControllerBase
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private bool doNotUseExpenseFilter;
        private bool doNotUseIncomeFilter;

        public OverallPerformanceController([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.doNotUseExpenseFilter = true;
            this.bucketRepository = bucketRepository;
        }

        public OverallPerformanceBudgetAnalyser Analysis { get; private set; }

        public bool ExpenseFilter
        {
            get { return this.doNotUseExpenseFilter; }

            set
            {
                this.doNotUseExpenseFilter = value;
                RaisePropertyChanged(() => ExpenseFilter);
                RefreshCollection();
            }
        }

        public bool IncomeFilter
        {
            get { return this.doNotUseIncomeFilter; }

            set
            {
                this.doNotUseIncomeFilter = value;
                RaisePropertyChanged(() => IncomeFilter);
                RefreshCollection();
            }
        }

        public double OverallPerformance { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required by data binding")]
        public string Title
        {
            get { return "Overall Budget Performance"; }
        }

        public void Load(StatementModel statementModel, BudgetCollection budgets, GlobalFilterCriteria criteria)
        {
            Analysis = new OverallPerformanceBudgetAnalyser(statementModel, budgets, this.bucketRepository);
            Analysis.Analyse(criteria);
            OverallPerformance = (double)Analysis.OverallPerformance;
            ExpenseFilter = true;
            IncomeFilter = false;

            RaisePropertyChanged(() => Analysis);
            ICollectionView view = CollectionViewSource.GetDefaultView(Analysis.Analyses);
            view.Filter = x =>
            {
                var bucketAnalysis = x as BucketPerformanceAnalyser;
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
            if (Analysis == null || Analysis.Analyses == null || !Analysis.Analyses.Any())
            {
                return;
            }

            CollectionViewSource.GetDefaultView(Analysis.Analyses).Refresh();
        }
    }
}