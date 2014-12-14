using System.Collections.ObjectModel;

namespace BudgetAnalyser.Engine.Widgets
{
    public class WidgetGroup
    {
        public const string GlobalFilterSectionName = "Global Filter";
        public const string OverviewSectionName = "Overview";
        public const string MonthlyTrackingSectionName = "Monthly Tracking";
        public const string ProjectsSectionName = "Projects";

        public string Heading { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for UI Binding.")]
        public ObservableCollection<Widget> Widgets { get; set; }

        public int Sequence { get; set; }
    }
}