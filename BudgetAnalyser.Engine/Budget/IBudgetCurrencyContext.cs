using System;

namespace BudgetAnalyser.Engine.Budget
{
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