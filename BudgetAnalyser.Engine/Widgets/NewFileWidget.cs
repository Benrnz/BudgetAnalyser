using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Widgets
{
    public class NewFileWidget : Widget
    {
        public NewFileWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[] { typeof(ApplicationDatabase) };
            Size = WidgetSize.Small;
            WidgetStyle = "ModernTileSmallStyle1";
            Clickable = true;
            DetailedText = "Create new";
            ImageResourceName = "NewFileImage";
            Sequence = 10;
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

            var appDb = input[0] as ApplicationDatabase;
            ToolTip = "Create a new Budget Analyser File.";
            DetailedText = "Create new";
            ColourStyleName = appDb == null ? WidgetWarningStyle : WidgetStandardStyle;
        }
    }
}