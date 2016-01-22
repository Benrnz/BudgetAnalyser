namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    public class FakeEnvironmentFolders : IEnvironmentFolders
    {
        /// <summary>
        /// Get the folder to store applications state data.
        /// </summary>
        public string ApplicationDataFolder()
        {
            throw new System.NotSupportedException();
        }

        /// <summary>
        /// Gets the folder to store diagnostic Logs.
        /// </summary>
        public string LogFolder()
        {
            throw new System.NotSupportedException();
        }
    }
}
