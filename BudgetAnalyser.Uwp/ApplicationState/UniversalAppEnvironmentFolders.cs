using System;
using System.Threading.Tasks;
using Windows.Storage;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.Uwp.ApplicationState
{
    internal class UniversalAppEnvironmentFolders : IEnvironmentFolders
    {
        /// <summary>
        ///     Get the folder to store applications state data.
        /// </summary>
        public async Task<string> ApplicationDataFolder()
        {
            var root = ApplicationData.Current.LocalFolder;
            var newFolder = await root.CreateFolderAsync("ApplicationState", CreationCollisionOption.OpenIfExists);
            return newFolder.Path;
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