using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Widgets
{
    public class WidgetGroup
    {
        public const string GlobalFilterSectionName = "Global Filter";
        public const string MonthlyTrackingSectionName = "Monthly Tracking";
        public const string OverviewSectionName = "Overview";
        public const string ProjectsSectionName = "Projects";
        public string Heading { get; set; }
        public int Sequence { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for UI Binding.")]
        public ObservableCollection<Widget> Widgets { get; set; }
    }
}