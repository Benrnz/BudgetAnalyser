using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Monitors the currently loaded Budget Analyser file and shows the file name.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
[UsedImplicitly] // Instantiated by Widget Service/Repo
public class CurrentFileWidget : Widget
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CurrentFileWidget" /> class.
    /// </summary>
    public CurrentFileWidget()
    {
        Category = WidgetGroup.OverviewSectionName;
        Dependencies = [typeof(ApplicationDatabase)];
        Size = WidgetSize.Medium;
        WidgetStyle = "ModernTileMediumStyle1";
        Clickable = true;
        DetailedText = "Loading...";
        ColourStyleName = WidgetWarningStyle;
        ImageResourceName = "FolderOpenImage";
        Sequence = 20;
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

        if (input[0] is not ApplicationDatabase appDb || string.IsNullOrEmpty(appDb.FileName))
        {
            ColourStyleName = WidgetWarningStyle;
            DetailedText = "Open";
            ToolTip = "Open an existing Budget Analyser File.";
        }
        else
        {
            ColourStyleName = WidgetStandardStyle;
            DetailedText = ShortenFileName(appDb.FileName);
            ToolTip = appDb.FileName;
        }
    }

    private static string ShortenFileName(string fileName)
    {
        if (fileName is null)
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        if (fileName.Length < 30)
        {
            return fileName;
        }

        var proposed = Path.GetFileName(fileName);
        var drive = Path.GetPathRoot(fileName);
        return proposed.Length < 30 ? drive + "...\\" + proposed : drive + "...\\" + proposed.Substring(proposed.Length - 30);
    }
}
