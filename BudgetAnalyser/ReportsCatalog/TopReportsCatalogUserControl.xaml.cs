using System.Windows;
using System.Windows.Controls;

namespace BudgetAnalyser.ReportsCatalog;

/// <summary>
///     Interaction logic for TopReportsCatalogUserControl.xaml
/// </summary>
public partial class TopReportsCatalogUserControl : UserControl
{
    public TopReportsCatalogUserControl()
    {
        InitializeComponent();
    }

    private void OverallPerformanceClicked(object? sender, RoutedEventArgs e)
    {
        ((TopReportsCatalogController)DataContext).ShowOverallPerformanceReport();
    }
}
