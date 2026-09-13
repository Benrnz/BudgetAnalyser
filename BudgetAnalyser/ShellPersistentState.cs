using System.Windows;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser;

public record ShellPersistentState : IPersistentApplicationStateObject
{
    /// <summary>
    ///     Default page size for paginated lists and grids.
    /// </summary>
    public int ListPageSize { get; init; }

    public Point Size { get; init; }
    public Point TopLeft { get; init; }
    public int LoadSequence => 1;
}
