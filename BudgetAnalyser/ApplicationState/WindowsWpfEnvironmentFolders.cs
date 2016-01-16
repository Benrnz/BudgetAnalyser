using System.IO;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.ApplicationState
{
    [AutoRegisterWithIoC]
    public class WindowsWpfEnvironmentFolders : IEnvironmentFolders
    {
        /// <summary>
        ///     Get the folder to store applications state data.
        /// </summary>
        public string ApplicationDataFolder()
        {
            return Path.GetDirectoryName(GetType().Assembly.Location);
        }

        /// <summary>
        ///     Gets the folder to store diagnostic Logs.
        /// </summary>
        public string LogFolder()
        {
            return Path.GetDirectoryName(GetType().Assembly.Location);
        }
    }
}
