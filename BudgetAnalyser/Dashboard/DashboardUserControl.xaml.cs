using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BudgetAnalyser.Dashboard
{
    /// <summary>
    ///     Interaction logic for DashboardUserControl.xaml
    /// </summary>
    public partial class DashboardUserControl : UserControl
    {
        public DashboardUserControl()
        {
            InitializeComponent();
        }

private  DashboardController Controller => (DashboardController)DataContext;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(Controller.WidgetGroups);
            if (view == null)
            {
                return;
            }

            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
        }
    }
}