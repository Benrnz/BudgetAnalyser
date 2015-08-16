using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC]
    public class OverallPerformanceChartService : IOverallPerformanceChartService
    {
        private readonly OverallPerformanceBudgetAnalyser analyser;

        public OverallPerformanceChartService([NotNull] OverallPerformanceBudgetAnalyser analyser)
        {
            if (analyser == null)
            {
                throw new ArgumentNullException(nameof(analyser));
            }

            this.analyser = analyser;
        }

        public OverallPerformanceBudgetResult BuildChart(StatementModel statementModel, BudgetCollection budgets, GlobalFilterCriteria criteria)
        {
            if (statementModel == null)
            {
                throw new ArgumentNullException(nameof(statementModel));
            }

            if (budgets == null)
            {
                throw new ArgumentNullException(nameof(budgets));
            }

            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            return this.analyser.Analyse(statementModel, budgets, criteria);
        }
    }
}