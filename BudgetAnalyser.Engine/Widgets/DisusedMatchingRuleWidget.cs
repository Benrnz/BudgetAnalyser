using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Monitors the number disused matching rules.  The more matching rules there are the slower the auto matching process
///     is. This widget helps find unused rules so they can be cleaned up.
/// </summary>
/// <seealso cref="Widget" />
[UsedImplicitly] // Instantiated by Widget Service/Repo
public class DisusedMatchingRuleWidget : Widget
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DisusedMatchingRuleWidget" /> class.
    /// </summary>
    public DisusedMatchingRuleWidget()
    {
        Category = WidgetGroup.OverviewSectionName;
        Dependencies = [typeof(ITransactionRuleService)];
        DetailedText = "Disused Matching Rules";
        RecommendedTimeIntervalUpdate = TimeSpan.FromMinutes(1);
        Clickable = true;
    }

    private static DateTime CutOffDate { get; set; }

    /// <summary>
    ///     Gets the list of disused matching rules.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global Used in Xaml binding
    public IEnumerable<MatchingRule> DisusedMatchingRules { get; private set; } = Array.Empty<MatchingRule>();

    /// <summary>
    ///     Returns a query that filters to disused rules
    /// </summary>
    public static IEnumerable<MatchingRule> QueryRules(IEnumerable<MatchingRule> allRules)
    {
        return allRules.Where(r => (r.MatchCount == 0 && r.Created < CutOffDate) || (r.MatchCount > 0 && r.LastMatch < CutOffDate));
    }

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    /// <exception cref="System.ArgumentNullException" />
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

        if (input[0] is not ITransactionRuleService ruleService)
        {
            Enabled = false;
            return;
        }

        CutOffDate = DateTime.Today.AddMonths(-18);
        var rulesList = QueryRules(ruleService.MatchingRules).ToList();
        DisusedMatchingRules = rulesList;
        var count = rulesList.Count();
        LargeNumber = count.ToString();
        ToolTip = $"{count}/{ruleService.MatchingRules.Count()} Rules that have not been used for more than a year.";
        ColourStyleName = count >= 20 ? WidgetWarningStyle : WidgetStandardStyle;
    }
}
