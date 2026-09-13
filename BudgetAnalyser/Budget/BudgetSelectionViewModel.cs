using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Budget;

public class BudgetSelectionViewModel(BudgetCollection budgets)
{
    public BudgetCollection Budgets { get; private set; } = budgets;

    public BudgetModel? Selected { get; set; }
}
