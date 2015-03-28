using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    /// An interface to provide access to retreive, store, and create <see cref="LedgerBook"/>s.
    /// </summary>
    public interface ILedgerBookRepository
    {
        Task<LedgerBook> CreateNewAsync([NotNull] string storageKey);
        Task<LedgerBook> LoadAsync([NotNull] string storageKey);
        Task SaveAsync([NotNull] LedgerBook book, [NotNull] string storageKey);
    }
}