using System.Globalization;

namespace BudgetAnalyser.Engine.Widget
{
    public class AccountFilterWidget : Widget
    {
        public AccountFilterWidget()
        {
            Category = "Global Filter";
            Dependencies = new[] { typeof(GlobalFilterCriteria) };
            ImageResourceName = "DateFilterBeakerImage";
            Size = WidgetSize.Medium;
            WidgetStyle = "ModernTileMediumStyle2";
            Clickable = true;
        }

        public override void Update(params object[] input)
        {
            if (!ValidateUpdateInput(input))
            {
                return;
            }

            var criteria = (GlobalFilterCriteria)input[0];
            if (criteria.Cleared)
            {
                DetailedText = "No Account filter applied.";
            }
            else if (criteria.AccountType != null)
            {
                DetailedText = string.Format(
                    CultureInfo.CurrentCulture,
                    "Filtered to show only {0} accounts",
                    criteria.AccountType);
            }
            else
            {
                DetailedText = "No Account filter applied.";
            }
        }
    }
}