using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC]
    internal class OverallPerformanceChartService(OverallPerformanceBudgetAnalyser analyser) : IOverallPerformanceChartService
    {
        private readonly OverallPerformanceBudgetAnalyser analyser = analyser ?? throw new ArgumentNullException(nameof(analyser));

        public OverallPerformanceBudgetResult BuildChart(StatementModel statementModel, BudgetCollection budgets, GlobalFilterCriteria criteria)
        {
            if (statementModel is null)
            {
                throw new ArgumentNullException(nameof(statementModel));
            }

            if (budgets is null)
            {
                throw new ArgumentNullException(nameof(budgets));
            }

            if (criteria is null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            return this.analyser.Analyse(statementModel, budgets, criteria);
        }
    }
}
