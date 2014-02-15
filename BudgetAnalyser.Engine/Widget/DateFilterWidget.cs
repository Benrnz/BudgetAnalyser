using System.Globalization;

namespace BudgetAnalyser.Engine.Widget
{
    public class DateFilterWidget : Widget
    {
        public DateFilterWidget()
        {
            Category = "Global Filter";
            Dependencies = new[] { typeof(GlobalFilterCriteria) };
            ImageResourceName = "DateFilterBeakerImage";
            Size = WidgetSize.Medium;
            WidgetStyle = "ModernTileMediumStyle1";
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
                DetailedText = "No date filter applied.";
            }
            else if (criteria.BeginDate != null)
            {
                DetailedText = string.Format(
                    CultureInfo.CurrentCulture,
                    "Filtered from {0} to {1}",
                    criteria.BeginDate.Value.ToShortDateString(),
                    criteria.EndDate.Value.ToShortDateString());
            }
            else
            {
                DetailedText = "No date filter applied.";
            }
        }
    }
}