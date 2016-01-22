namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An interface to retrieve environment specific folder locations.  These locations and the means to get them will
    ///     vary from platform to platform.
    /// </summary>
    public interface IEnvironmentFolders
    {
        /// <summary>
        ///     Get the folder to store applications state data.
        /// </summary>
        string ApplicationDataFolder();

        /// <summary>
        ///     Gets the folder to store diagnostic Logs.
        /// </summary>
        string LogFolder();
    }
}