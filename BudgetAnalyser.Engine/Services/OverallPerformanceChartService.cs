using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services;

[AutoRegisterWithIoC]
[UsedImplicitly] // Used by IoC
internal class OverallPerformanceChartService(OverallPerformanceBudgetAnalyser analyser) : IOverallPerformanceChartService
{
    private readonly OverallPerformanceBudgetAnalyser analyser = analyser ?? throw new ArgumentNullException(nameof(analyser));

    public OverallPerformanceBudgetResult BuildChart(TransactionSetModel transactionSetModel, BudgetCollection budgets, GlobalFilterCriteria criteria)
    {
        if (transactionSetModel is null)
        {
            throw new ArgumentNullException(nameof(transactionSetModel));
        }

        if (budgets is null)
        {
            throw new ArgumentNullException(nameof(budgets));
        }

        if (criteria is null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }

        return this.analyser.Analyse(transactionSetModel, budgets, criteria);
    }
}
