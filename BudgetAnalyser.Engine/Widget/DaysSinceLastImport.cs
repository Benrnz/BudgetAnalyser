using System;
using System.Globalization;

namespace BudgetAnalyser.Engine.Widget
{
    public class DaysSinceLastImport : Widget
    {
        public DaysSinceLastImport()
        {
            Category = "Transactions";
            Dependencies = new[] { typeof(StatementModel) };
            DetailedText = "Days since last import";
            ImageResourceName = null;
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
            Clickable = true;
        }

        public override void Update(params object[] input)
        {
            if (!ValidateUpdateInput(input))
            {
                return;
            }

            var statement = (StatementModel)input[0];
            int days = Convert.ToInt32(DateTime.Today.Subtract(statement.Imported).TotalDays);
            if (days < 0)
            {
                days = 0;
            }

            LargeNumber = days.ToString(CultureInfo.CurrentCulture);
            ToolTip = string.Format("It's been {0} days since new transactions have been imported.", LargeNumber);
            if (days >= 7)
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