using System;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A transient helper to indicate a budget's active state in relation to other budgets in the
    ///     collection.
    ///     This class will tell you if a budget is active, future dated, or has past and is archived.
    ///     This class has no state of its own it calculates all of its properties.
    /// </summary>
    public interface IBudgetCurrencyContext
    {
        /// <summary>
        ///     Gets a boolean value to indicate if this is the most recent and currently active <see cref="BudgetModel" />.
        /// </summary>
        bool BudgetActive { get; }

        bool BudgetArchived { get; }
        bool BudgetInFuture { get; }
        DateTime? EffectiveUntil { get; }
        string FileName { get; }
        BudgetModel Model { get; }
    }
}