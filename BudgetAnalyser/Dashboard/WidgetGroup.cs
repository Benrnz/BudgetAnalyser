using System.Collections.ObjectModel;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Dashboard
{
    public class WidgetGroup
    {
        public string Heading { get; set; }

        public ObservableCollection<Widget> Widgets { get; set; }
    }
}