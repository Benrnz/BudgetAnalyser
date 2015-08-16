namespace BudgetAnalyser.Engine.Persistence
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<BudgetAnalyserStorageRoot, ApplicationDatabase>))]
    public class StorageRootToApplicationDatabaseMapper : MagicMapper<BudgetAnalyserStorageRoot, ApplicationDatabase>
    {
    }
}