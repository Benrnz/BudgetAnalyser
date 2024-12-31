using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A grouping of widget for use in the UI. Used to group similar purpose widgets together for ease of use.
    /// </summary>
    public class WidgetGroup
    {
        /// <summary>
        ///     A constant for the global filter section name
        /// </summary>
        public const string GlobalFilterSectionName = "Global Filter";

        /// <summary>
        ///     A constant for the monthly tracking section name
        /// </summary>
        public const string PeriodicTrackingSectionName = "Tracking";

        /// <summary>
        ///     A constant for the overview section name
        /// </summary>
        public const string OverviewSectionName = "Overview";

        /// <summary>
        ///     A constant for the projects section name
        /// </summary>
        public const string ProjectsSectionName = "Projects";

        /// <summary>
        ///     Gets or sets the group heading.
        /// </summary>
        public string Heading { get; set; }

        /// <summary>
        ///     Gets or sets the sequence.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        ///     Gets or sets the widgets in this group.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Required for UI Binding.")]
        public ObservableCollection<Widget> Widgets { get; set; }
    }
}
