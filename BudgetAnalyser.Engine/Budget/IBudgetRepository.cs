using System;
using System.Collections.Generic;
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
        ///     Any existing data at this location will be overwritten. After this is complete, use the <see cref="LoadAsync" /> method
        ///     to load the new collection.
        /// </summary>
        /// <param name="storageKey">The unique storage identifier</param>
        Task<BudgetCollection> CreateNewAndSaveAsync([NotNull] string storageKey);

        /// <summary>
        ///     Loads the a <see cref="BudgetCollection" /> from storage at the location indicated by <see paramref="storageKey" />.
        /// </summary>
        /// <param name="storageKey">The unique storage identifier</param>
        /// <param name="isEncrypted">A boolean to indicate if the data file should be encrypted or not.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException">File not found.  + storageKey</exception>
        /// <exception cref="DataFormatException">
        ///     Deserialisation the Budget file failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.
        /// </exception>
        Task<BudgetCollection> LoadAsync([NotNull] string storageKey, bool isEncrypted);

        /// <summary>
        ///     Saves the current <see cref="BudgetCollection" /> to storage.
        ///     Uses the previously set storageKey. 
        ///     The file is assumed to be encrypted if it was loaded from an encrypted file.
        /// </summary>
        Task SaveAsync();

        /// <summary>
        ///     Saves the current <see cref="BudgetCollection" /> to storage.
        ///     Allows saving to a different storage location by setting a different key.
        /// </summary>
        /// <param name="storageKey">The unique storage identifier</param>
        /// <param name="isEncrypted">A boolean to indicate if the data file should be encrypted or not.</param>
        Task SaveAsync(string storageKey, bool isEncrypted);
    }
}