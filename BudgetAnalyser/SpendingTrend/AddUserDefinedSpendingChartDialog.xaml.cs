using System.Windows;

namespace BudgetAnalyser.SpendingTrend
{
    /// <summary>
    ///     Interaction logic for AddUserDefinedSpendingChartDialog.xaml
    /// </summary>
    public partial class AddUserDefinedSpendingChartDialog : Window
    {
        public AddUserDefinedSpendingChartDialog()
        {
            InitializeComponent();
        }

        private void AddChartOnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}