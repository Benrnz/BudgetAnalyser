using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     A service interface to manage all aspects of using the Overall Performance Chart Report
/// </summary>
public interface IOverallPerformanceChartService : IServiceFoundation
{
    /// <summary>
    ///     Builds the chart.
    /// </summary>
    /// <param name="transactionSetModel">The current statement model.</param>
    /// <param name="budgets">The current budgets.</param>
    /// <param name="criteria">The criteria.</param>
    /// <returns>A data result object that contains the results of the analysis ready for binding in the UI.</returns>
    OverallPerformanceBudgetResult BuildChart(TransactionSetModel transactionSetModel, BudgetCollection budgets, GlobalFilterCriteria criteria);
}
