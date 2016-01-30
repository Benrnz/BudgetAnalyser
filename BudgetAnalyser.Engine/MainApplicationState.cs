using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     The stored application state for the Main view.
    ///     This is saved when the application exits.
    /// </summary>
    public class MainApplicationState : IPersistentApplicationStateObject
    {
        /// <summary>
        ///     Gets or sets a string that indicates where the budget analyser stores its data.
        /// </summary>
        public string BudgetAnalyserDataStorageKey { get; set; }

        /// <summary>
        ///     Gets the sequence number for this implementation.
        ///     This is used to load more crucial higher priority persistent data first, if any.
        /// </summary>
        public int LoadSequence => 1;
    }
}