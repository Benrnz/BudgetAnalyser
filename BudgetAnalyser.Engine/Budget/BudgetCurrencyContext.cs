using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A transient helper wrapper class to indicate a budget's active state in relation to other budgets in the
    ///     collection.
    ///     This class will tell you if a budget is active, future dated, or has past and is archived.
    ///     This class has no state of its own it calculates all of its properties.
    /// </summary>
    public class BudgetCurrencyContext : IBudgetCurrencyContext
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="BudgetCurrencyContext" /> class.
        /// </summary>
        /// <param name="budgets">The collection of available budgets loaded.</param>
        /// <param name="budget">
        ///     The currently selected budget. This isn't necessarily the current one compared with today's date.
        ///     Can be any in the <paramref name="budgets" /> collection.
        /// </param>
        public BudgetCurrencyContext([NotNull] BudgetCollection budgets, [NotNull] BudgetModel budget)
        {
            if (budgets == null)
            {
                throw new ArgumentNullException(nameof(budgets));
            }

            if (budget == null)
            {
                throw new ArgumentNullException(nameof(budget));
            }

            BudgetCollection = budgets;
            Model = budget;

            if (budgets.IndexOf(budget) < 0)
            {
                throw new KeyNotFoundException("The given budget is not found in the given collection.");
            }
        }

        /// <summary>
        ///     Gets a boolean value to indicate if this is the most recent and currently active <see cref="BudgetModel" />.
        /// </summary>
        public bool BudgetActive => BudgetCollection.IsCurrentBudget(Model);

        public bool BudgetArchived => BudgetCollection.IsArchivedBudget(Model);
        public BudgetCollection BudgetCollection { get; }
        public bool BudgetInFuture => BudgetCollection.IsFutureBudget(Model);

        public DateTime? EffectiveUntil
        {
            get
            {
                int myIndex = BudgetCollection.IndexOf(Model);
                if (myIndex == 0)
                {
                    // There is no superceding budget, so this budget is effective indefinitely (no expiry date).
                    // I'd rather return null here because returning 31/12/9999 is just weird, and looks stupid when displayed to user.
                    return null;
                }

                BudgetModel nextBudget = BudgetCollection[myIndex - 1];
                return nextBudget.EffectiveFrom;
            }
        }

        public string FileName => BudgetCollection.StorageKey;
        public virtual BudgetModel Model { get; }
    }
}