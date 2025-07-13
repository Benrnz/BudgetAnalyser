using System.Globalization;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A widget to counter and test the scheduled updates that should occur at defined regular time intervals.
/// </summary>
[UsedImplicitly] // Instantiated by Widget Service / Repo
public class TimedUpdateCounterWidget : Widget
{
    private int refreshCount;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TimedUpdateCounterWidget" /> class.
    /// </summary>
    public TimedUpdateCounterWidget()
    {
        Category = WidgetGroup.OverviewSectionName;
        Dependencies =
        [
            typeof(TransactionSetModel),
            typeof(BudgetCollection),
            typeof(IBudgetCurrencyContext),
            typeof(LedgerBook),
            typeof(IBudgetBucketRepository),
            typeof(GlobalFilterCriteria),
            typeof(LedgerCalculation)
        ];
        RecommendedTimeIntervalUpdate = TimeSpan.FromSeconds(100);
        DetailedText = "Refresh Count";
        Name = "Refresh Interval Counter";
        ToolTip = "Counts the number of refreshes received by any widget as well as refreshing every 100 seconds.";
    }

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    public override void Update(params object[] input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        this.refreshCount++;
        Enabled = true;
        LargeNumber = this.refreshCount.ToString(CultureInfo.CurrentCulture);
    }
}
