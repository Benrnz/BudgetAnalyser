using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Persistence
{
    /// <summary>
    ///     This is the unified single master repository that returns a database model that contains references to all other
    ///     models.
    /// </summary>
    public interface IApplicationDatabaseRepository
    {
        Task<ApplicationDatabase> CreateNewAsync(string storageKey);
        Task<ApplicationDatabase> LoadAsync([NotNull] string storageKey);
        Task SaveAsync([NotNull] ApplicationDatabase budgetAnalyserDatabase);
    }
}