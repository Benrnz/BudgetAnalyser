using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service to prepare and present data ready for convenient consumption by the Budget Pie Graph.
    /// </summary>
    public interface IBudgetPieGraphService : IServiceFoundation
    {
        /// <summary>
        ///     Prepares the expense graph data.
        /// </summary>
        IDictionary<string, decimal> PrepareExpenseGraphData(BudgetModel budget);

        /// <summary>
        ///     Prepares the income graph data.
        /// </summary>
        IDictionary<string, decimal> PrepareIncomeGraphData(BudgetModel budget);

        /// <summary>
        ///     A model Surplus expense object for the UI to bind to.
        /// </summary>
        Expense SurplusExpense(BudgetModel model);
    }
}