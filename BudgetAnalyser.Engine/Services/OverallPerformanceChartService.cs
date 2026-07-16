using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Transactions;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services;

[AutoRegisterWithIoC]
[UsedImplicitly] // Used by IoC
internal class OverallPerformanceChartService(OverallPerformanceBudgetAnalyser analyser) : IOverallPerformanceChartService
{
    // TODO this class has no reason to exist
    private readonly OverallPerformanceBudgetAnalyser analyser = analyser ?? throw new ArgumentNullException(nameof(analyser));

    public OverallPerformanceBudgetResult BuildChart(TransactionsListModel transactions, BudgetCollection budgets, DateOnly startDate, DateOnly endDate)
    {
        if (transactions is null)
        {
            throw new ArgumentNullException(nameof(transactions));
        }

        if (budgets is null)
        {
            throw new ArgumentNullException(nameof(budgets));
        }

        return this.analyser.Analyse(transactions, budgets, startDate, endDate);
    }
}
