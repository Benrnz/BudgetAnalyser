using System.Globalization;

namespace BudgetAnalyser.Engine.Widget
{
    public class DateFilterWidget : Widget
    {
        private string doNotUseImageResourceName2;

        public DateFilterWidget()
        {
            Category = "Global Filter";
            Dependencies = new[] { typeof(GlobalFilterCriteria) };
            ImageResourceName = "DateFilterBeakerImage";
            ImageResourceName2 = "DateFilterCalendarImage";
            Size = WidgetSize.Medium;
            WidgetStyle = "ModernTileMediumStyle1";
            Clickable = true;
        }

        // This is a little hacky.  Need to figure out how to combine the ImageSource for both images into one.
        public string ImageResourceName2
        {
            get { return this.doNotUseImageResourceName2; }
            set
            {
                this.doNotUseImageResourceName2 = value;
                OnPropertyChanged();
            }
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