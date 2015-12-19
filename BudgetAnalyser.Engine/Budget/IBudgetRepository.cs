using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     An interface to describe interaction with persistence functions for retrieving and saving
    ///     <see cref="BudgetCollection" />s. This is designed to be a state-aware.
    /// </summary>
    public interface IBudgetRepository
    {
        /// <summary>
        ///     Creates a new empty <see cref="BudgetCollection" /> but does not save it.
        /// </summary>
        BudgetCollection CreateNew();

        /// <summary>
        ///     Creates a new empty <see cref="BudgetCollection" /> at the location indicated by the <see paramref="storageKey" />.
        ///     Any
        ///     existing data at this location will be overwritten. After this is complete, use the <see cref="LoadAsync" /> method
        ///     to load the new collection.
        /// </summary>
        Task<BudgetCollection> CreateNewAndSaveAsync([NotNull] string storageKey);

        /// <summary>
        ///     Loads the a <see cref="BudgetCollection" /> from storage at the location indicated by <see paramref="storageKey" />
        ///     .
        /// </summary>
        Task<BudgetCollection> LoadAsync([NotNull] string storageKey);

        /// <summary>
        ///     Saves the current <see cref="BudgetCollection" /> to storage.
        /// </summary>
        Task SaveAsync();
    }
}