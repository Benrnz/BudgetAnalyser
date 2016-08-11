using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     An interface to provide access to retreive, store, and create <see cref="LedgerBook" />s.
    /// </summary>
    public interface ILedgerBookRepository
    {
        /// <summary>
        ///     Creates a new empty <see cref="LedgerBook" /> at the location indicated by the <paramref name="storageKey" />. Any
        ///     existing data at this location will be overwritten. After this is complete, use the <see cref="LoadAsync" /> method
        ///     to
        ///     load the new <see cref="LedgerBook" />.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Will be thrown if arguments are null.</exception>
        Task<LedgerBook> CreateNewAndSaveAsync([NotNull] string storageKey);

        /// <summary>
        ///     Loads the Ledger Book from persistent storage.
        /// </summary>
        /// <param name="isEncrypted">A boolean to indicate if the data file should be encrypted or not.</param>
        /// <param name="storageKey">The unique storage identifier</param>
        /// <exception cref="System.ArgumentNullException">Will be thrown if null arguments are passed.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Will be thrown if the data file can not be found on the disk</exception>
        /// <exception cref="DataFormatException">
        ///     Will be thrown if deserialisation of the Ledger Book file failed.
        ///     Or an exception was thrown by the Xaml deserialiser.
        ///     Or the file format is invalid.
        ///     Or the Ledger Book has been tampered with based on the checksum.
        /// </exception>
        Task<LedgerBook> LoadAsync([NotNull] string storageKey, bool isEncrypted);

        /// <summary>
        ///     Saves the Ledger Book to the location indicated by the storage key. Any existing Ledger Book at that location will
        ///     be overwritten.
        /// </summary>
        /// <param name="book">The Ledger Book object to save.</param>
        /// <param name="storageKey">The unique storage identifier</param>
        /// <param name="isEncrypted">A boolean to indicate if the data file should be encrypted or not.</param>
        /// <exception cref="System.ArgumentNullException">Will be thrown if arguments are null.</exception>
        Task SaveAsync([NotNull] LedgerBook book, [NotNull] string storageKey, bool isEncrypted);
    }
}