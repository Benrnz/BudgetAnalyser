using BudgetAnalyser.Engine.Budget;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    public class BudgetReadyMessage(IBudgetCurrencyContext activeBudget, BudgetCollection? budgets = null) : MessageBase
    {
        public IBudgetCurrencyContext ActiveBudget { get; private set; } = activeBudget;
        public BudgetCollection? Budgets { get; private set; } = budgets;
    }
}
