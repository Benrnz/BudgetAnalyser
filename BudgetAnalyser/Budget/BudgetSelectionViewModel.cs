using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Budget
{
    public class BudgetSelectionViewModel
    {
        public BudgetSelectionViewModel(BudgetCollection budgets)
        {
            Budgets = budgets;
        }

        public BudgetCollection Budgets { get; private set; }

        public BudgetModel Selected { get; set; }
    }
}
