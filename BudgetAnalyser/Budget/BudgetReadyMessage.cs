using BudgetAnalyser.Engine.Budget;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Budget
{
    public class BudgetReadyMessage : MessageBase
    {
        public BudgetReadyMessage(IBudgetCurrencyContext activeBudget, BudgetCollection budgets = null)
        {
            ActiveBudget = activeBudget;
            Budgets = budgets;
        }

        public IBudgetCurrencyContext ActiveBudget { get; private set; }
        public BudgetCollection Budgets { get; private set; }
    }
}