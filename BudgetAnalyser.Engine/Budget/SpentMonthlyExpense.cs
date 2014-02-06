namespace BudgetAnalyser.Engine.Budget
{
    public class SpentMonthlyExpense : ExpenseBudgetBucket
    {
        public SpentMonthlyExpense()
        {
        }

        public SpentMonthlyExpense(string code, string name)
            : base(code, name)
        {
        }
    }
}