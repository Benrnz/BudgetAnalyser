using System.IO;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.ApplicationState
{
    [AutoRegisterWithIoC]
    public class WindowsWpfEnvironmentFolders : IEnvironmentFolders
    {
        /// <summary>
        ///     Get the folder to store applications state data.
        /// </summary>
        public Task<string> ApplicationDataFolder()
        {
            return Task.FromResult(Path.GetDirectoryName(GetType().Assembly.Location));
        }

        /// <summary>
        ///     Gets the folder to store diagnostic Logs.
        /// </summary>
        public Task<string> LogFolder()
        {
            return Task.FromResult(Path.GetDirectoryName(GetType().Assembly.Location));
        }
    }
}
