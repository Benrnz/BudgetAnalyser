using System.ComponentModel;
using System.Windows;

namespace BudgetAnalyser.Budget
{
    /// <summary>
    ///     Interaction logic for BudgetMaintenance.xaml
    /// </summary>
    public partial class BudgetMaintenance 
    {
        public BudgetMaintenance()
        {
            InitializeComponent();
        }

        private BudgetController Controller
        {
            get { return DataContext as BudgetController; }
        }

        private void OnExpensesListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            if (listChangedEventArgs.ListChangedType == ListChangedType.ItemAdded)
            {
                this.ExpensesListScrollViewer.ScrollToBottom();
                int count = this.Expenses.Items.Count == 0 ? 0 : this.Expenses.Items.Count - 1;
                this.Expenses.SelectedIndex = count;
            }
        }

        private void OnOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var controller = e.OldValue as BudgetController;
                if (controller != null && controller.Expenses != null)
                {
                    controller.Expenses.ListChanged -= OnExpensesListChanged;
                }
            }

            if (Controller != null && Controller.Expenses != null)
            {
                Controller.Expenses.ListChanged += OnExpensesListChanged;
            }
        }
    }
}