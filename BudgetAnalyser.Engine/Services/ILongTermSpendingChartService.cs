using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    /// A service interface to provide all functionality for the Long Term Spending Analysis Chart.
    /// This service is not stateful, and is not threadsafe.
    /// </summary>
    public interface ILongTermSpendingChartService : IServiceFoundation
    {
        /// <summary>
        /// Builds the chart.
        /// </summary>
        /// <param name="statementModel">The current statement model.</param>
        /// <param name="criteria">The current criteria.</param>
        /// <returns>A graph object ready for binding and use in the UI.</returns>
        GraphData BuildChart([NotNull] StatementModel statementModel, [NotNull] GlobalFilterCriteria criteria);
    }
}
