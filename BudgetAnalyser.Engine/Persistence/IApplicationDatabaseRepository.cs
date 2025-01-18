namespace BudgetAnalyser.Engine.Persistence;

/// <summary>
///     This is the unified single master repository that returns a database model that contains references to all other
///     models.
/// </summary>
internal interface IApplicationDatabaseRepository
{
    Task<ApplicationDatabase> CreateNewAsync(string storageKey);
    Task<ApplicationDatabase> LoadAsync(string storageKey);
    Task SaveAsync(ApplicationDatabase budgetAnalyserDatabase);
}
