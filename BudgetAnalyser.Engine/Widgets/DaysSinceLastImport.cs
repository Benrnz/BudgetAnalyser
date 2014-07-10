using System;
using System.Globalization;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets
{
    public class DaysSinceLastImport : Widget
    {
        public DaysSinceLastImport()
        {
            Category = "Files";
            Dependencies = new[] { typeof(StatementModel) };
            DetailedText = "Days since last import";
            ImageResourceName = null;
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
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
                Enabled = false;
                return;
            }

            Enabled = true;
            var statement = (StatementModel)input[0];
            int days = Convert.ToInt32(DateTime.Today.Subtract(statement.LastImport).TotalDays);
            if (days < 0)
            {
                days = 0;
            }

            LargeNumber = days.ToString(CultureInfo.CurrentCulture);
            ToolTip = string.Format(CultureInfo.CurrentCulture, "It's been {0} days since new transactions have been imported.", LargeNumber);
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