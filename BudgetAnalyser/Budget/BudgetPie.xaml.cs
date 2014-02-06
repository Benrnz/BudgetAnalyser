using System.Collections.Generic;
using System.Windows.Controls;

namespace BudgetAnalyser.Budget
{
    /// <summary>
    ///     Interaction logic for BudgetPie.xaml
    /// </summary>
    public partial class BudgetPie : UserControl
    {
        public BudgetPie()
        {
            InitializeComponent();
        }

        private void OnExpenseChartSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0)
            {
                return;
            }

            var selectedItem = (KeyValuePair<string, decimal>)e.AddedItems[0];
            ((BudgetPieController)DataContext).ExpenseSelectedItem = selectedItem;
        }

        private void OnIncomeChartSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0)
            {
                return;
            }

            var selectedItem = (KeyValuePair<string, decimal>)e.AddedItems[0];
            ((BudgetPieController)DataContext).IncomeSelectedItem = selectedItem;
        }
    }
}