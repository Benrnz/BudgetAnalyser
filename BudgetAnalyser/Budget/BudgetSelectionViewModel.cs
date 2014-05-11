using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Budget
{
    public class BudgetSelectionViewModel
    {
        public BudgetCollection Budgets { get; set; }

        public BudgetModel Selected { get; set; }
    }
}
