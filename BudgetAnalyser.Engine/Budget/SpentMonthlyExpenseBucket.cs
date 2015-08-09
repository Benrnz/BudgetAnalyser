namespace BudgetAnalyser.Engine.Budget
{
    public class SpentMonthlyExpenseBucket : BillToPayExpenseBucket
    {
        public SpentMonthlyExpenseBucket()
        {
            // Default constructor required for deserialisation.
        }

        public SpentMonthlyExpenseBucket(string code, string name)
            : base(code, name)
        {
        }

        public override string TypeDescription => "Spent Monthly Expense";
    }
}