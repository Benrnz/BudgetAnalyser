using System.Globalization;
using BudgetAnalyser.Engine.Transactions;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Monitors the number of days since bank transactions were imported.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
[UsedImplicitly] // Instantiated by Widget Service/Repo
public class DaysSinceLastImport : Widget
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DaysSinceLastImport" /> class.
    /// </summary>
    public DaysSinceLastImport()
    {
        Category = WidgetGroup.OverviewSectionName;
        Dependencies = [typeof(TransactionsListModel)];
        DetailedText = "Days since last import";
        RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
        Clickable = true;
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

        if (!ValidateUpdateInput(input))
        {
            Enabled = false;
            return;
        }

        Enabled = true;
        var transactions = (TransactionsListModel)input[0];
        var days = Convert.ToInt32(DateTime.Today.Subtract(transactions.LastImport).TotalDays);
        if (days < 0)
        {
            days = 0;
        }

        LargeNumber = days > 99 ? "99+" : days.ToString(CultureInfo.CurrentCulture);
        ToolTip = $"It's been {LargeNumber} days since new transactions have been imported.";
        ColourStyleName = days >= 7 ? WidgetWarningStyle : WidgetStandardStyle;
    }
}
