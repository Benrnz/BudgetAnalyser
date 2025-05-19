using System.Windows;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.ApplicationState;

public record BaxApplicationStateDto
{
    public BaxApplicationStateDto()
    {
        ShellWindowState = new ShellWindowStateDto(new Point(1100, 1200), new Point(0, 0), 30);
        LastBaxFile = string.Empty;
    }

    public BaxApplicationStateDto(IPersistentApplicationStateObject[] states)
    {
        var shell = states.OfType<ShellPersistentState>().Single();
        ShellWindowState = new ShellWindowStateDto(shell.Size, shell.TopLeft, shell.ListPageSize);
        var main = states.OfType<ApplicationEngineState>().Single();
        LastBaxFile = main.BudgetAnalyserDataStorageKey;
    }

    public string LastBaxFile { get; init; }
    public ShellWindowStateDto ShellWindowState { get; init; }
}

public record ShellWindowStateDto(Point Size, Point TopLeft, int ListPageSize);
