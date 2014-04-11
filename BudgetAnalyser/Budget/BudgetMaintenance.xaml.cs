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

        private void OnIncomesListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            if (listChangedEventArgs.ListChangedType == ListChangedType.ItemAdded)
            {
                this.IncomesListScrollViewer.ScrollToBottom();
                int count = this.Incomes.Items.Count == 0 ? 0 : this.Incomes.Items.Count - 1;
                this.Incomes.SelectedIndex = count;
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var controller = e.OldValue as BudgetController;
                if (controller != null)
                {
                    if (controller.Expenses != null)
                    {
                        controller.Expenses.ListChanged -= OnExpensesListChanged;
                    }

                    if (controller.Incomes != null)
                    {
                        controller.Incomes.ListChanged -= OnIncomesListChanged;
                    }
                }
            }

            if (Controller != null)
            {
                if (Controller.Expenses != null)
                {
                    Controller.Expenses.ListChanged += OnExpensesListChanged;
                }

                if (Controller.Incomes != null)
                {
                    Controller.Incomes.ListChanged += OnIncomesListChanged;
                }
            }
        }
    }
}