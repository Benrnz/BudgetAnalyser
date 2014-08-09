using BudgetAnalyser.Engine.Budget;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Budget
{
    public class BudgetReadyMessage : MessageBase
    {
        public BudgetReadyMessage(BudgetCurrencyContext activeBudget, BudgetCollection budgets = null)
        {
            ActiveBudget = activeBudget;
            Budgets = budgets;
        }

        public BudgetCurrencyContext ActiveBudget { get; private set; }

        public BudgetCollection Budgets { get; private set; }
    }
}