using System;
using System.Collections.Generic;
using System.Text;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service to maintain the full collection of budgets.
    /// </summary>
    public interface IBudgetMaintenanceService : IServiceFoundation
    {
        /// <summary>
        ///     Gets the budget bucket repository.
        ///     Allows the UI to set up a static reference to the Bucket repository for converters and templates.
        /// </summary>
        IBudgetBucketRepository BudgetBucketRepository { get; }

        /// <summary>
        ///     Clones the given <see cref="BudgetModel" /> to create a new budget with a future effective date.
        /// </summary>
        /// <param name="sourceBudget">The source budget to clone from.</param>
        /// <param name="newBudgetEffectiveFrom">This date will be used as the new budget's effective date.</param>
        /// <returns>The newly created budget.</returns>
        /// <exception cref="ArgumentNullException">Will be thrown if source budget is null.</exception>
        /// <exception cref="ValidationWarningException">Will be thrown if the source budget is in an invalid state.</exception>
        /// <exception cref="ArgumentException">
        ///     Will be thrown if the effective date of the new budget is not after the provided
        ///     budget.
        /// </exception>
        /// <exception cref="ArgumentException">Will be thrown if the effective date is not a future date.</exception>
        BudgetModel CloneBudgetModel([NotNull] BudgetModel sourceBudget, DateTime newBudgetEffectiveFrom);

        /// <summary>
        ///     Creates a new budget collection with one new empty budget model.
        /// </summary>
        BudgetCurrencyContext CreateNewBudgetCollection();

        /// <summary>
        ///     Loads the collection of budgets from persistent storage.
        /// </summary>
        /// <param name="storageKey">The storage key to identify the budget collection.</param>
        /// <returns>
        ///     An object that contains the full collection of available budgets as well as the most recent selected as the
        ///     currently selected budget.
        /// </returns>
        BudgetCurrencyContext LoadBudgetsCollection(string storageKey);

        /// <summary>
        ///     Saves the budget provided to persistent storage.
        /// </summary>
        /// <param name="modifiedBudget">The modified budget.</param>
        /// <param name="comment">The optional comment.</param>
        /// <returns>true if the budget was saved successfully, otherwise false - this indicates there are validation errors.</returns>
        bool SaveBudget(BudgetModel modifiedBudget, string comment = "");

        /// <summary>
        ///     Updates the budget with the full collection of incomes and expenses and then validates the budget.
        /// </summary>
        /// <param name="model">The Budget model.</param>
        /// <param name="allIncomes">All income objects that may have changed in the UI.</param>
        /// <param name="allExpenses">All expense objects that may have changed in the UI.</param>
        /// <param name="validationMessages">The validation messages. Will be blank if no issues are found.</param>
        /// <returns>
        ///     True if valid, otherwise false.  If false is returned there will be addition information in the
        ///     <paramref name="validationMessages" /> string builder.
        /// </returns>
        bool UpdateAndValidateBudget(BudgetModel model, IEnumerable<Income> allIncomes, IEnumerable<Expense> allExpenses, StringBuilder validationMessages);
    }
}