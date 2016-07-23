using System;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Enables or disables encryption for files saved to disk.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
    public sealed class EncryptWidget : Widget
    {
        private ApplicationDatabase appDb;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentFileWidget" /> class.
        /// </summary>
        public EncryptWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[] { typeof(ApplicationDatabase) };
            Clickable = true;
            Enabled = false;
            ColourStyleName = WidgetStandardStyle;
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

            var newAppDb = (ApplicationDatabase) input[0];
            if (newAppDb == null)
            {
                this.appDb = null;
            }
            else
            {
                this.appDb = newAppDb;
            }

            if (this.appDb == null) return;
            WidgetActivated();
        }

        /// <summary>
        ///     Is called by the UI when this widget is clicked or activated.
        /// </summary>
        public void WidgetActivated()
        {
            if (this.appDb == null) return;
            Enabled = true;
            if (this.appDb.IsEncrypted)
            {
                ColourStyleName = WidgetStandardStyle;
                ImageResourceName = "LockClosedImage";
                DetailedText = "Encrypted";
            }
            else
            {
                ColourStyleName = WidgetWarningStyle;
                ImageResourceName = "LockOpenImage";
                DetailedText = "Unprotected";
            }
        }
    }
}