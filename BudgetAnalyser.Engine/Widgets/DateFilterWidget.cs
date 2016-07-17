using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     The date filter widget.  This widget offers access to change the global filtering criteria as well as showing the
    ///     current filter criteria in the UI.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.IWidgetWithAdditionalImage" />
    public class DateFilterWidget : Widget, IWidgetWithAdditionalImage
    {
        private readonly string standardStyleName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateFilterWidget" /> class.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Reviewed ok here")]
        public DateFilterWidget()
        {
            Category = WidgetGroup.GlobalFilterSectionName;
            Dependencies = new[] { typeof(GlobalFilterCriteria) };
            ImageResourceName = "DateFilterImage";
            Size = WidgetSize.Medium;
            this.standardStyleName = "Brush.ModernTile.Background2";
            WidgetStyle = "ModernTileMediumStyle2";
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
                return;
            }

            var criteria = (GlobalFilterCriteria) input[0];
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
                criteria.BeginDate?.Date.ToString("d"),
                criteria.EndDate?.Date.ToString("d"));
        }

        private void NoDateFilterApplied()
        {
            DetailedText = "No date filter applied.";
            ColourStyleName = WidgetWarningStyle;
        }
    }
}