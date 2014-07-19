using System.Collections.ObjectModel;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Dashboard
{
    public class WidgetGroup
    {
        public string Heading { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for UI Binding.")]
        public ObservableCollection<Widget> Widgets { get; set; }
    }
}