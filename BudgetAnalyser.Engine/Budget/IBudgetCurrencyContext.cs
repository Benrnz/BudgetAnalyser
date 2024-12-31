using System;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A transient helper to indicate a budget's active state in relation to other budgets in the
    ///     collection. This class will tell you if a budget is active, future dated, or has past and is archived. This class
    ///     has no state of its own it calculates all of its properties.
    /// </summary>
    public interface IBudgetCurrencyContext
    {
        /// <summary>
        ///     Gets a boolean value to indicate if this is the most recent and currently active <see cref="BudgetModel" />.
        /// </summary>
        bool BudgetActive { get; }

        /// <summary>
        ///     Gets a value indicating whether the budget is archived.
        /// </summary>
        bool BudgetArchived { get; }

        /// <summary>
        ///     Gets a value indicating whether the budget will be applicable in the future.
        /// </summary>
        bool BudgetInFuture { get; }

        /// <summary>
        ///     Gets the effective until date. This is the last date the budget will be applicable before another budget comes into
        ///     affect.
        /// </summary>
        DateTime? EffectiveUntil { get; }

        /// <summary>
        ///     Gets the name of the file.
        /// </summary>
        // TODO This should be renamed to StorageKey
        string FileName { get; }

        /// <summary>
        ///     Gets the budget model.
        /// </summary>
        BudgetModel Model { get; }
    }
}
