using System;
using System.Globalization;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Monitors the number of days since bank statement data was last imported.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
    public class DaysSinceLastImport : Widget
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DaysSinceLastImport" /> class.
        /// </summary>
        public DaysSinceLastImport()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[] { typeof(StatementModel) };
            DetailedText = "Days since last import";
            ImageResourceName = null;
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
            Clickable = true;
        }

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public override void Update([NotNull] params object[] input)
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
            var statement = (StatementModel) input[0];
            var days = Convert.ToInt32(DateTime.Today.Subtract(statement.LastImport).TotalDays);
            if (days < 0)
            {
                days = 0;
            }

            LargeNumber = days > 99 ? "99+" : days.ToString(CultureInfo.CurrentCulture);
            ToolTip = string.Format(CultureInfo.CurrentCulture,
                "It's been {0} days since new transactions have been imported.", LargeNumber);
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