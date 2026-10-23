using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A widget to load the built-in demo Budget Analyser file.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
[UsedImplicitly] // Instantiated by the Widget Service / Repo.
public class LoadDemoWidget : Widget
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LoadDemoWidget" /> class.
    /// </summary>
    public LoadDemoWidget()
    {
        Category = WidgetGroup.OverviewSectionName;
        Dependencies = [];
        DetailedText = "Load Demo";
        ImageResourceName = "SmileyImage";
        Clickable = true;
        Sequence = 25;
    }

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    /// <exception cref="System.ArgumentNullException"></exception>
    public override void Update(params object[] input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        Enabled = true;
        ToolTip = "Load a demo to show the basic features of Budget Analyser.";
    }
}
