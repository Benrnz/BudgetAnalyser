using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service to maintain the full collection of budgets.
    ///     This is designed as a stateful service.
    /// </summary>
    public interface IBudgetMaintenanceService : INotifyDatabaseChanges, IServiceFoundation
    {
        /// <summary>
        ///     Gets the budget bucket repository.
        ///     Allows the UI to set up a static reference to the Bucket repository for converters and templates.
        /// </summary>
        IBudgetBucketRepository BudgetBucketRepository { get; }

        /// <summary>
        ///     Gets the curently loaded budgets collection.
        /// </summary>
        BudgetCollection Budgets { get; }

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
        ///     Updates the budget with the full collection of incomes and expenses.
        ///     This method does not call validate on the model.
        /// </summary>
        /// <param name="model">The Budget model.</param>
        /// <param name="allIncomes">All income objects that may have changed in the UI.</param>
        /// <param name="allExpenses">All expense objects that may have changed in the UI.</param>
        void UpdateIncomesAndExpenses(BudgetModel model, IEnumerable<Income> allIncomes,
            IEnumerable<Expense> allExpenses);
    }
}