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

        private DashboardController Controller
        {
            get { return (DashboardController)DataContext; }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            var view = CollectionViewSource.GetDefaultView(Controller.Widgets);
            if (view == null)
            {
                return;
            }

            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
        }
    }
}