using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Enables or disables encryption for files saved to disk.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
[UsedImplicitly] // Instantiated by Widget Service/Repo
public sealed class EncryptWidget : Widget
{
    private ApplicationDatabase? appDb;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CurrentFileWidget" /> class.
    /// </summary>
    public EncryptWidget()
    {
        Category = WidgetGroup.OverviewSectionName;
        Dependencies = [typeof(ApplicationDatabase)];
        Clickable = true;
        Enabled = false;
        ColourStyleName = WidgetStandardStyle;
        ImageResourceName = "LockOpenImage";
        Sequence = 11;
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

        this.appDb = input[0] as ApplicationDatabase;
        if (this.appDb is null)
        {
            return;
        }

        Enabled = true;
        if (this.appDb.IsEncrypted)
        {
            ColourStyleName = WidgetStandardStyle;
            ImageResourceName = "LockClosedImage";
            DetailedText = "Encrypted";
        }
        else
        {
            ColourStyleName = WidgetWarningStyle;
            ImageResourceName = "LockOpenImage";
            DetailedText = "Unprotected";
        }
    }
}
