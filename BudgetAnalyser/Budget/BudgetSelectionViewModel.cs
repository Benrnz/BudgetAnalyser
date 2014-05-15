using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Budget
{
    public class BudgetSelectionViewModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection")]
        public BudgetSelectionViewModel(BudgetCollection budgets)
        {
            Budgets = budgets;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification="Custom collection")]
        public BudgetCollection Budgets { get; private set; }

        public BudgetModel Selected { get; set; }
    }
}
