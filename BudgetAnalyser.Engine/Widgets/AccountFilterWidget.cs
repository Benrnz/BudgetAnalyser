using System;
using System.Globalization;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    public class AccountFilterWidget : Widget
    {
        public AccountFilterWidget()
        {
            Category = WidgetGroup.GlobalFilterSectionName;
            Dependencies = new[] { typeof(GlobalFilterCriteria) };
            ImageResourceName = "DateFilterBeakerImage";
            Size = WidgetSize.Medium;
            WidgetStyle = "ModernTileMediumStyle2";
            Clickable = true;
        }

        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!ValidateUpdateInput(input))
            {
                return;
            }

            var criteria = (GlobalFilterCriteria)input[0];
            if (criteria.Cleared)
            {
                DetailedText = "No Account filter applied.";
            }
            else if (criteria.Account != null)
            {
                DetailedText = string.Format(
                    CultureInfo.CurrentCulture,
                    "Filtered to show only {0} accounts",
                    criteria.Account);
            }
            else
            {
                DetailedText = "No Account filter applied.";
            }
        }
    }
}