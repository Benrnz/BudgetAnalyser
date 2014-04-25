using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    public class BudgetAnalysisController : ControllerBase
    {
        private bool doNotUseExpenseFilter;
        private bool doNotUseIncomeFilter;

        public BudgetAnalysisController()
        {
            this.doNotUseExpenseFilter = true;
        }

        public OverallPerformanceBudgetAnalysis Analysis { get; private set; }

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

        public void Load(OverallPerformanceBudgetAnalysis overallPerformanceBudgetAnalysis)
        {
            Analysis = overallPerformanceBudgetAnalysis;
            OverallPerformance = (double)overallPerformanceBudgetAnalysis.OverallPerformance;
            ExpenseFilter = true;
            IncomeFilter = false;

            ICollectionView view = CollectionViewSource.GetDefaultView(Analysis.Analyses);
            view.Filter = x =>
            {
                var bucketAnalysis = x as BucketPerformanceAnalysis;
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