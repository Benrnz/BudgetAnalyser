using BudgetAnalyser.Engine.Budget;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Budget
{
    public class BudgetReadyMessage : MessageBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification="Custom Collection")]
        public BudgetReadyMessage(BudgetCurrencyContext activeBudget, BudgetCollection budgets = null)
        {
            ActiveBudget = activeBudget;
            Budgets = budgets;
        }

        public BudgetCurrencyContext ActiveBudget { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification="Custom Collection")]
        public BudgetCollection Budgets { get; private set; }
    }
}