using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    public class DateFilterWidget : Widget, IWidgetWithAdditionalImage
    {
        private readonly string standardStyleName;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Reviewed ok here")]
        public DateFilterWidget()
        {
            Category = WidgetGroup.GlobalFilterSectionName;
            Dependencies = new[] { typeof(GlobalFilterCriteria) };
            ImageResourceName = "DateFilterBeakerImage";
            ImageResourceName2 = "DateFilterCalendarImage";
            Size = WidgetSize.Medium;
            this.standardStyleName = "Brush.ModernTile.Background2";
            WidgetStyle = "ModernTileMediumStyle2";
            Clickable = true;
        }

        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (!ValidateUpdateInput(input))
            {
                return;
            }

            var criteria = (GlobalFilterCriteria)input[0];
            if (criteria.Cleared)
            {
                NoDateFilterApplied();
            }
            else if (criteria.BeginDate != null)
            {
                DateFilterApplied(criteria);
            }
            else
            {
                NoDateFilterApplied();
            }
        }

        private void DateFilterApplied(GlobalFilterCriteria criteria)
        {
            ColourStyleName = this.standardStyleName;
            DetailedText = string.Format(
                CultureInfo.CurrentCulture,
                "Filtered from {0} to {1}",
                criteria.BeginDate.Value.ToShortDateString(),
                criteria.EndDate.Value.ToShortDateString());
        }

        private void NoDateFilterApplied()
        {
            DetailedText = "No date filter applied.";
            ColourStyleName = WidgetWarningStyle;
        }
    }
}