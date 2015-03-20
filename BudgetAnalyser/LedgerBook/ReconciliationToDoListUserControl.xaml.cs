using System.Windows;
using System.Windows.Controls;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    ///     Interaction logic for ReconciliationToDoListUserControl.xaml
    /// </summary>
    public partial class ReconciliationToDoListUserControl : UserControl
    {
        public ReconciliationToDoListUserControl()
        {
            InitializeComponent();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var controller = DataContext as ReconciliationToDoListController;
            if (controller == null)
            {
                return;
            }

            controller.Close();
        }
    }
}