using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A transient wrapper class to indicate a budget's active state in relation to other budgets in the collection.
    ///     This class will tell you if a budget is active, future dated, or has past and is archived.
    /// </summary>
    public class BudgetCurrencyContext : IBudgetCurrencyContext
    {
        private readonly BudgetCollection budgets;

        /// <summary>
        /// Creates a new instance of the <see cref="BudgetCurrencyContext"/> class.
        /// </summary>
        /// <param name="budgets">The collection of available budgets loaded.</param>
        /// <param name="budget">The currently selected budget. This isn't necessarily the current one compared with today's date. Can be any in the <paramref name="budgets"/> collection.</param>
        public BudgetCurrencyContext([NotNull] BudgetCollection budgets, [NotNull] BudgetModel budget)
        {
            if (budgets == null)
            {
                throw new ArgumentNullException("budgets");
            }

            if (budget == null)
            {
                throw new ArgumentNullException("budget");
            }

            this.budgets = budgets;
            Model = budget;

            if (budgets.IndexOf(budget) < 0)
            {
                throw new KeyNotFoundException("The given budget is not found in the given collection.");
            }
        }

        /// <summary>
        ///     Gets a boolean value to indicate if this is the most recent and currently active <see cref="BudgetModel" />.
        /// </summary>
        public bool BudgetActive
        {
            get { return this.budgets.IsCurrentBudget(Model); }
        }

        public bool BudgetArchived
        {
            get { return this.budgets.IsArchivedBudget(Model); }
        }

        public bool BudgetInFuture
        {
            get { return this.budgets.IsFutureBudget(Model); }
        }

        public DateTime? EffectiveUntil
        {
            get
            {
                int myIndex = this.budgets.IndexOf(Model);
                if (myIndex == 0)
                {
                    // There is no superceding budget, so this budget is effective indefinitely (no expiry date).
                    // I'd rather return null here because returning 31/12/9999 is just weird, and looks stupid when displayed to user.
                    return null;
                }

                BudgetModel nextBudget = this.budgets[myIndex - 1];
                return nextBudget.EffectiveFrom;
            }
        }

        public string FileName
        {
            get { return this.budgets.FileName; }
        }

        public virtual BudgetModel Model { get; private set; }
    }
}