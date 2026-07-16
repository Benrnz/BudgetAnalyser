using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     A service interface to manage all aspects of using the Overall Performance Chart Report
/// </summary>
public interface IOverallPerformanceChartService : IServiceFoundation
{
    /// <summary>
    ///     Builds the chart.
    /// </summary>
    /// <param name="transactions">The current transactions model.</param>
    /// <param name="budgets">The current budgets.</param>
    /// <param name="startDate">The start date for the analysis.</param>
    /// <param name="endDate">The end date for the analysis.</param>
    /// <returns>A data result object that contains the results of the analysis ready for binding in the UI.</returns>
    OverallPerformanceBudgetResult BuildChart(TransactionsListModel transactions, BudgetCollection budgets, DateOnly startDate, DateOnly endDate);
}
