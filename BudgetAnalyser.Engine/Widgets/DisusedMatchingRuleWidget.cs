using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Monitors the number disused matching rules.  The more matching rules there are the slower the auto matching process
    ///     is.
    ///     This widget helps find unused rules so they can be cleaned up.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
    public class DisusedMatchingRuleWidget : Widget
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisusedMatchingRuleWidget" /> class.
        /// </summary>
        public DisusedMatchingRuleWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[] { typeof(ITransactionRuleService) };
            DetailedText = "Disused Matching Rules";
            ImageResourceName = null;
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
            Clickable = true;
        }

        /// <summary>
        ///     Gets the list of disused matching rules.
        /// </summary>
        public IEnumerable<MatchingRule> DisusedMatchingRules { get; private set; }

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public override void Update(params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!ValidateUpdateInput(input))
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            var ruleService = (ITransactionRuleService) input[0];
            DateTime cutOffDate = DateTime.Today.AddYears(-1);
            DisusedMatchingRules = ruleService.MatchingRules.Where(r => (r.MatchCount == 0 && r.Created < cutOffDate)
                                                                        || (r.MatchCount > 0 && r.LastMatch < cutOffDate));

            var count = DisusedMatchingRules.Count();
            LargeNumber = count.ToString();
            ToolTip = "Rules that have not been used for more than a year.";
            if (count >= 10)
            {
                ColourStyleName = WidgetWarningStyle;
            }
            else
            {
                ColourStyleName = WidgetStandardStyle;
            }
        }
    }
}