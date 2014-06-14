using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using BudgetAnalyser.Engine.Reports;

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

        private LongTermSpendingGraphController Controller
        {
            get { return (LongTermSpendingGraphController)DataContext; }
        }

        private void OnChartDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            UpdateChart();
        }

        private void UpdateChart()
        {
            foreach (SeriesData seriesData in Controller.Graph.Series.Where(s => s.Visible))
            {
                var series = new LineSeries
                {
                    DependentValuePath = "Amount",
                    IndependentValuePath = "Month",
                    IsSelectionEnabled = true,
                    DataContext = seriesData,
                };

                var plotsBinding = new Binding
                {
                    Path = new PropertyPath("Plots"),
                };
                series.SetBinding(DataPointSeries.ItemsSourceProperty, plotsBinding);

                var titleBinding = new Binding
                {
                    Path = new PropertyPath("SeriesName"),
                };
                series.SetBinding(Series.TitleProperty, titleBinding);

                var visibilityBinding = new Binding
                {
                    Path = new PropertyPath("Visible"),
                    Converter = new BooleanToVisibilityConverter(),
                };
                series.SetBinding(UIElement.VisibilityProperty, visibilityBinding);

                this.Chart.Series.Add(series);
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                Controller.NotifyOfClose();
            }
        }
    }
}