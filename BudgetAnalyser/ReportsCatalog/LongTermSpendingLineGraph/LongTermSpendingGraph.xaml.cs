using System.Windows;
using System.Windows.Controls;

namespace BudgetAnalyser.ReportsCatalog.LongTermSpendingLineGraph
{
    /// <summary>
    ///     Interaction logic for LongTermSpendingGraph.xaml
    /// </summary>
    public partial class LongTermSpendingGraph : UserControl
    {
        public LongTermSpendingGraph()
        {
            InitializeComponent();
        }

        private LongTermSpendingGraphController Controller => (LongTermSpendingGraphController)DataContext;

        private void OnChartDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            UpdateChart();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                Controller.NotifyOfClose();
            }
        }

        private void SeriesOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
 
            
        }

        private void UpdateChart()
        {
            //foreach (SeriesData seriesData in Controller.Graph.Series.Where(s => s.Visible))
            //{


            //}
        }
    }
}