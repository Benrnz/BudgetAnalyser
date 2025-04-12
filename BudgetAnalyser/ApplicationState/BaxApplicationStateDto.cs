using System.Windows;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.ApplicationState;

public record BaxApplicationStateDto
{
    public BaxApplicationStateDto()
    {
        ShellWindowState = new ShellWindowStateDto { Size = new Point(1100, 1200), TopLeft = new Point(0, 0) };
        LastBaxFile = string.Empty;
    }

    public BaxApplicationStateDto(IPersistentApplicationStateObject[] states)
    {
        var shell = states.OfType<ShellPersistentState>().Single();
        ShellWindowState = new ShellWindowStateDto { Size = shell.Size, TopLeft = shell.TopLeft };
        var main = states.OfType<MainApplicationState>().Single();
        LastBaxFile = main.BudgetAnalyserDataStorageKey;
    }

    public string LastBaxFile { get; init; }
    public ShellWindowStateDto ShellWindowState { get; init; }
}

public record ShellWindowStateDto
{
    public required Point Size { get; init; }
    public required Point TopLeft { get; init; }
}
