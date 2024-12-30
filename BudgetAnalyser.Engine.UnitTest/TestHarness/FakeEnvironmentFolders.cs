using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    public class FakeEnvironmentFolders : IEnvironmentFolders
    {
        /// <summary>
        /// Get the folder to store applications state data.
        /// </summary>
        public Task<string> ApplicationDataFolder()
        {
            throw new System.NotSupportedException();
        }

        /// <summary>
        /// Gets the folder to store diagnostic Logs.
        /// </summary>
        public Task<string> LogFolder()
        {
            throw new System.NotSupportedException();
        }
    }
}