namespace BudgetAnalyser.Engine.Budget
{
    public class SpentMonthlyExpense : ExpenseBudgetBucket
    {
        public SpentMonthlyExpense(string code, string name)
            : base(code, name)
        {
        }

        public override string TypeDescription
        {
            get { return "Spent Monthly Expense"; }
        }
    }
}