using System;
using System.Threading.Tasks;
using Windows.Storage;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.Uwp.ApplicationState
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class UniversalAppEnvironmentFolders : IEnvironmentFolders
    {
        private string cachedAppDataFolder;

        /// <summary>
        ///     Get the folder to store applications state data.
        /// </summary>
        public async Task<string> ApplicationDataFolder()
        {
            if (string.IsNullOrWhiteSpace(this.cachedAppDataFolder))
            {
                var root = ApplicationData.Current.LocalFolder;
                var newFolder = await root.CreateFolderAsync("ApplicationState", CreationCollisionOption.OpenIfExists);
                this.cachedAppDataFolder = newFolder.Path;
            }

            return this.cachedAppDataFolder;
        }

        /// <summary>
        ///     Gets the folder to store diagnostic Logs.
        /// </summary>
        public async Task<string> LogFolder()
        {
            return await ApplicationDataFolder();
        }
    }
}