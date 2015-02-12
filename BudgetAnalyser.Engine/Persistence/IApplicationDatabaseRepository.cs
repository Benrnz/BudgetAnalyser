namespace BudgetAnalyser.Engine.Persistence
{
    /// <summary>
    ///     This is the unified single master repository that returns a database model that contains all models.
    /// </summary>
    public interface IApplicationDatabaseRepository
    {
        ApplicationDatabase Load(MainApplicationStateModelV1 stateModel);
    }
}