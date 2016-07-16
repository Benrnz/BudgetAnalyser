using System;
using BudgetAnalyser.Engine.Services;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A widget that becomes available when there are unsaved changes. Clicking will save all changes.
    /// </summary>
    /// <seealso cref="Widget" />
    public class SaveWidget : Widget
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SaveWidget" /> class.
        /// </summary>
        public SaveWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[] { typeof(IApplicationDatabaseService) };
            DetailedText = "Save";
            ImageResourceName = "SaveImage";
            RecommendedTimeIntervalUpdate = TimeSpan.FromSeconds(30);
            Clickable = true;
            Sequence = 12;
        }

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        public override void Update(params object[] input)
        {
            Enabled = false;
            Clickable = false;
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!ValidateUpdateInput(input))
            {
                return;
            }

            var appDbService = (IApplicationDatabaseService) input[0];
            if (appDbService == null) return;

            Enabled = appDbService.HasUnsavedChanges;
            Clickable = Enabled;
            DetailedText = Enabled ? "Save" : "No changes";
        }
    }
}