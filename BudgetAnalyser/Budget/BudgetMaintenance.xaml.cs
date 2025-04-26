using System.ComponentModel;
using System.Windows;

namespace BudgetAnalyser.Budget;

/// <summary>
///     Interaction logic for BudgetMaintenance.xaml
/// </summary>
public partial class BudgetMaintenance
{
    public BudgetMaintenance()
    {
        InitializeComponent();
    }

    private BudgetController Controller => (BudgetController)DataContext;

    private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is BudgetController controller)
        {
            controller.Expenses.ListChanged -= OnExpensesListChanged;
            controller.Incomes.ListChanged -= OnIncomesListChanged;
        }

        Controller.Expenses.ListChanged += OnExpensesListChanged;
        Controller.Incomes.ListChanged += OnIncomesListChanged;
    }

    private void OnExpensesListChanged(object? sender, ListChangedEventArgs listChangedEventArgs)
    {
        if (listChangedEventArgs.ListChangedType == ListChangedType.ItemAdded)
        {
            this.ExpensesListScrollViewer.ScrollToBottom();
            var count = this.Expenses.Items.Count == 0 ? 0 : this.Expenses.Items.Count - 1;
            this.Expenses.SelectedIndex = count;
        }
    }

    private void OnIncomesListChanged(object? sender, ListChangedEventArgs listChangedEventArgs)
    {
        if (listChangedEventArgs.ListChangedType == ListChangedType.ItemAdded)
        {
            this.IncomesListScrollViewer.ScrollToBottom();
            var count = this.Incomes.Items.Count == 0 ? 0 : this.Incomes.Items.Count - 1;
            this.Incomes.SelectedIndex = count;
        }
    }
}
