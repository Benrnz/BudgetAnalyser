using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Budget;

public class BudgetSelectionViewModel
{
    public BudgetSelectionViewModel(BudgetCollection budgets)
    {
        Budgets = budgets;
    }

    [UsedImplicitly]
    public BudgetCollection Budgets { get; private set; }

    [UsedImplicitly]
    public BudgetModel? Selected { get; set; }
}
