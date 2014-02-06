using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Budget
{
    public partial class BudgetSelectionView : Window
    {
        public BudgetSelectionView()
        {
            InitializeComponent();
        }

        private void OnBudgetClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                throw new InvalidOperationException("Expected ItemsControl item to be a button so click is captured.");
            }

            var budget = button.DataContext as BudgetModel;
            if (budget == null)
            {
                throw new InvalidOperationException("Expected DataContext of button to be a BudgetModel.");
            }

            Close();
            ((BudgetController)DataContext).ShowOtherBudget(budget);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var budgetController = e.NewValue as BudgetController;
                if (budgetController != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(budgetController.Budgets);
                    view.SortDescriptions.Add(new SortDescription("EffectiveFrom", ListSortDirection.Descending));
                }
            }
        }
    }
}