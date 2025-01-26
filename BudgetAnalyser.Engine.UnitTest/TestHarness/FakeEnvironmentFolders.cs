namespace BudgetAnalyser.Engine.UnitTest.TestHarness;

public class FakeEnvironmentFolders : IEnvironmentFolders
{
    /// <summary>
    ///     Get the folder to store applications state data.
    /// </summary>
    public Task<string> ApplicationDataFolder()
    {
        throw new NotSupportedException();
    }
}
