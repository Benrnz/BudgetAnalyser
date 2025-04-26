using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace BudgetAnalyser.LedgerBook;

/// <summary>
///     Interaction logic for ReconciliationToDoListUserControl.xaml
/// </summary>
public partial class ReconciliationToDoListUserControl
{
    public ReconciliationToDoListUserControl()
    {
        InitializeComponent();
    }

    private ReconciliationToDoListController Controller => (ReconciliationToDoListController)DataContext;

    private void OnListDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Controller.Tasks is null)
        {
            return;
        }

        var view = CollectionViewSource.GetDefaultView(Controller.Tasks);
        view.SortDescriptions.Add(new SortDescription("SystemGenerated", ListSortDirection.Descending));
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ReconciliationToDoListController controller)
        {
            return;
        }

        controller.Close();
    }
}
