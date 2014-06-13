using System.Windows;

namespace BudgetAnalyser.ReportsCatalog.BurnDownGraphs
{
    /// <summary>
    ///     Interaction logic for AddUserDefinedBurnDownDialog.xaml
    /// </summary>
    public partial class AddUserDefinedBurnDownDialog : Window
    {
        public AddUserDefinedBurnDownDialog()
        {
            InitializeComponent();
        }

        private void AddChartOnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
    }
}