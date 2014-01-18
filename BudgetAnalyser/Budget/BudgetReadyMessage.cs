using BudgetAnalyser.Engine.Budget;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Budget
{
    public class BudgetReadyMessage : MessageBase
    {
        public BudgetReadyMessage(BudgetCurrencyContext budget)
        {
            Budget = budget;
        }

        public BudgetCurrencyContext Budget { get; private set; }
    }
}