using System.Windows;
using System.Windows.Controls;

namespace BudgetAnalyser.ReportsCatalog
{
    /// <summary>
    ///     Interaction logic for ReportsCatalogUserControl.xaml
    /// </summary>
    public partial class ReportsCatalogUserControl : UserControl
    {
        public ReportsCatalogUserControl()
        {
            InitializeComponent();
        }

        private void OverallPerformanceClicked(object sender, RoutedEventArgs e)
        {
            ((ReportsCatalogController)DataContext).ShowOverallPerformanceReport();
        }
    }
}