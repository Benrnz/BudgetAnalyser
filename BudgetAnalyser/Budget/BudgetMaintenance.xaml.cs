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

        private BudgetController Controller => (BudgetController)DataContext;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is BudgetController controller)
            {
                if (controller.Expenses is not null)
                {
                    controller.Expenses.ListChanged -= OnExpensesListChanged;
                }

                if (controller.Incomes is not null)
                {
                    controller.Incomes.ListChanged -= OnIncomesListChanged;
                }
            }

            if (Controller.Expenses is not null)
            {
                Controller.Expenses.ListChanged += OnExpensesListChanged;
            }

            if (Controller.Incomes is not null)
            {
                Controller.Incomes.ListChanged += OnIncomesListChanged;
            }
        }

        private void OnExpensesListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            if (listChangedEventArgs.ListChangedType == ListChangedType.ItemAdded)
            {
                this.ExpensesListScrollViewer.ScrollToBottom();
                var count = this.Expenses.Items.Count == 0 ? 0 : this.Expenses.Items.Count - 1;
                this.Expenses.SelectedIndex = count;
            }
        }

        private void OnIncomesListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            if (listChangedEventArgs.ListChangedType == ListChangedType.ItemAdded)
            {
                this.IncomesListScrollViewer.ScrollToBottom();
                var count = this.Incomes.Items.Count == 0 ? 0 : this.Incomes.Items.Count - 1;
                this.Incomes.SelectedIndex = count;
            }
        }
    }
}
