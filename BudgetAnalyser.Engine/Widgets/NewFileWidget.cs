using System;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     The new file widget, shows information about creating a new Budget Analyser Database file.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
    public class NewFileWidget : Widget
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NewFileWidget" /> class.
        /// </summary>
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

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public override void Update([NotNull] params object[] input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!ValidateUpdateInput(input))
            {
                return;
            }

            ToolTip = "Create a new Budget Analyser File.";
            DetailedText = "Create new";
            ColourStyleName = input[0] is not ApplicationDatabase appDb ? WidgetWarningStyle : WidgetStandardStyle;
        }
    }
}
