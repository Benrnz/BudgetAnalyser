using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Persistence
{
    /// <summary>
    ///     This is the unified single master repository that returns a database model that contains references to all other
    ///     models.
    /// </summary>
    internal interface IApplicationDatabaseRepository
    {
        Task<ApplicationDatabase> CreateNewAsync(string storageKey);
        Task<ApplicationDatabase> LoadAsync([NotNull] string storageKey);
        Task SaveAsync([NotNull] ApplicationDatabase budgetAnalyserDatabase);
    }
}