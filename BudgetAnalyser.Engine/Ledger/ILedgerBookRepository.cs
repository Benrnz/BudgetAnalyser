using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    /// An interface to provide access to retreive, store, and create <see cref="LedgerBook"/>s.
    /// </summary>
    public interface ILedgerBookRepository
    {
        LedgerBook CreateNew(string name, string storageKey);
        bool Exists(string storageKey);
        Task<LedgerBook> LoadAsync(string storageKey);

        void Save(LedgerBook book);
        void Save(LedgerBook book, string storageKey);
    }
}