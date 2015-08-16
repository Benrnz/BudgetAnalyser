namespace BudgetAnalyser.Engine.Persistence
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<ApplicationDatabase, BudgetAnalyserStorageRoot>))]
    public class ApplicationDatabaseToStorageRootMapper : MagicMapper<ApplicationDatabase, BudgetAnalyserStorageRoot>
    {
    }
}