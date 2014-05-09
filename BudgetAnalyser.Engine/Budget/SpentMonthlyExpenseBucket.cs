namespace BudgetAnalyser.Engine.Budget
{
    public class SpentMonthlyExpenseBucket : ExpenseBudgetBucket
    {
        public SpentMonthlyExpenseBucket()
        {
            // Default constructor required for deserialisation.
        }

        public SpentMonthlyExpenseBucket(string code, string name)
            : base(code, name)
        {
        }

        public override string TypeDescription
        {
            get { return "Spent Monthly Expense"; }
        }
    }
}