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
        Task<LedgerBook> CreateNewAndSaveAsync([NotNull] string storageKey);

        /// <summary>
        ///     Loads the Ledger Book from persistent storage.
        /// </summary>
        Task<LedgerBook> LoadAsync([NotNull] string storageKey);

        /// <summary>
        ///     Saves the Ledger Book to the location indicated by the storage key. Any existing Ledger Book at that location will
        ///     be overwritten.
        /// </summary>
        Task SaveAsync([NotNull] LedgerBook book, [NotNull] string storageKey);
    }
}