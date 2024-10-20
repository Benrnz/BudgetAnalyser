using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        private ReconciliationToDoListController Controller => DataContext as ReconciliationToDoListController;

        private void OnListDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(Controller.Tasks);
            view.SortDescriptions.Add(new SortDescription("SystemGenerated", ListSortDirection.Descending));
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var controller = DataContext as ReconciliationToDoListController;
            if (controller is null)
            {
                return;
            }

            controller.Close();
        }
    }
}