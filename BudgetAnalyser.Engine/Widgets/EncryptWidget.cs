using System;
using System.IO;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Enables or disables encryption for files saved to disk.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
    public class EncryptWidget : Widget
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentFileWidget" /> class.
        /// </summary>
        public EncryptWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[] { typeof(ApplicationDatabase) };
            Clickable = true;
            DetailedText = "Password";
            ColourStyleName = WidgetWarningStyle;
            ImageResourceName = "LockOpenImage";
            Sequence = 11;
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

        }
    }
}