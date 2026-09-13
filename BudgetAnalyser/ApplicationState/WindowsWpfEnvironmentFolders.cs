using System.IO;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.ApplicationState;

[AutoRegisterWithIoC]
public class WindowsWpfEnvironmentFolders : IEnvironmentFolders
{
    /// <summary>
    ///     Get the folder to store applications state data.
    /// </summary>
    public Task<string> ApplicationDataFolder()
    {
        return Task.FromResult(Path.GetDirectoryName(GetType().Assembly.Location) ?? string.Empty);
    }
}
