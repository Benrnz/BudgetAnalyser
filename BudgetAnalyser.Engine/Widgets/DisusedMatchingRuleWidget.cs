using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Monitors the number disused matching rules.  The more matching rules there are the slower the auto matching process
    ///     is. This widget helps find unused rules so they can be cleaned up.
    /// </summary>
    /// <seealso cref="Widget" />
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
            RecommendedTimeIntervalUpdate = TimeSpan.FromMinutes(1);
            Clickable = true;
        }

        private static DateTime CutOffDate { get; set; }

        /// <summary>
        ///     Gets the list of disused matching rules.
        /// </summary>
        public IEnumerable<MatchingRule> DisusedMatchingRules { get; private set; }

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

            if (ruleService == null)
            {
                Enabled = false;
                return;
            }

            CutOffDate = DateTime.Today.AddMonths(-18);
            List<MatchingRule> rulesList = QueryRules(ruleService.MatchingRules).ToList();
            DisusedMatchingRules = rulesList;
            var count = rulesList.Count();
            LargeNumber = count.ToString();
            ToolTip = $"{count}/{ruleService.MatchingRules.Count()} Rules that have not been used for more than a year.";
            if (count >= 20)
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