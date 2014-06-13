using System;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.OverallPerformance;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetAnalysisBuilder : IBudgetAnalysisView
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly BudgetAnalysisController controller;

        public BudgetAnalysisBuilder([NotNull] BudgetAnalysisController controller, [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.controller = controller;
            this.bucketRepository = bucketRepository;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection type is necessary")]
        public virtual OverallPerformanceBudgetAnalyser Analyse(StatementModel statement, BudgetCollection budgets, GlobalFilterCriteria criteria)
        {
            var analysis = new OverallPerformanceBudgetAnalyser(statement, budgets, this.bucketRepository);
            analysis.Analyse(criteria);
            return analysis;
        }

        public virtual void ShowDialog(OverallPerformanceBudgetAnalyser analysis)
        {
            this.controller.Load(analysis);
            var window = new BudgetAnalysisView
            {
                DataContext = this.controller,
            };

            window.ShowDialog();
        }
    }
}