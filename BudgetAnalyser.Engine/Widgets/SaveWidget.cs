using BudgetAnalyser.Engine.Services;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A widget that becomes available when there are unsaved changes. Clicking will save all changes.
/// </summary>
/// <seealso cref="Widget" />
[UsedImplicitly] // Instantiated by the Widget Service / Repo.
public class SaveWidget : Widget
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SaveWidget" /> class.
    /// </summary>
    public SaveWidget()
    {
        Category = WidgetGroup.OverviewSectionName;
        Dependencies = [typeof(IDirtyDataService)];
        DetailedText = "Save";
        ImageResourceName = "SaveImage";
        RecommendedTimeIntervalUpdate = TimeSpan.FromSeconds(30);
        Clickable = true;
        Sequence = 12;
    }

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    public override void Update(params object[] input)
    {
        Enabled = false;
        Clickable = false;
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (!ValidateUpdateInput(input))
        {
            return;
        }

        var dirtyDataService = (IDirtyDataService)input[0];

        Enabled = dirtyDataService.HasUnsavedChanges;
        Clickable = Enabled;
        DetailedText = Enabled ? "Save" : "No changes";
    }
}
