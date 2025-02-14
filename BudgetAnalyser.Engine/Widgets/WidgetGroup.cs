using System.Collections.ObjectModel;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A grouping of widget for use in the UI. Used to group similar purpose widgets together for ease of use.
/// </summary>
public class WidgetGroup
{
    /// <summary>
    ///     A constant for the global filter section name
    /// </summary>
    public const string GlobalFilterSectionName = "Global Filter";

    /// <summary>
    ///     A constant for the monthly tracking section name
    /// </summary>
    public const string PeriodicTrackingSectionName = "Tracking";

    /// <summary>
    ///     A constant for the overview section name
    /// </summary>
    public const string OverviewSectionName = "Overview";

    /// <summary>
    ///     A constant for the projects section name
    /// </summary>
    public const string ProjectsSectionName = "Projects";

    internal static readonly Dictionary<string, int> GroupSequence = new()
    {
        { OverviewSectionName, 1 }, { GlobalFilterSectionName, 2 }, { PeriodicTrackingSectionName, 3 }, { ProjectsSectionName, 4 }
    };

    /// <summary>
    ///     Gets or sets the group heading.
    /// </summary>
    public required string Heading { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the sequence.
    /// </summary>
    public int Sequence { get; init; }

    /// <summary>
    ///     Gets or sets the widgets in this group.
    /// </summary>
    public required ObservableCollection<Widget> Widgets { get; init; }
}
