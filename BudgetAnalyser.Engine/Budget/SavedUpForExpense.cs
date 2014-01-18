namespace BudgetAnalyser.Engine.Budget
{
    public class SavedUpForExpense : ExpenseBudgetBucket
    {
        public SavedUpForExpense()
            : base()
        {
        }

        public SavedUpForExpense(string code, string name)
            : base(code, name)
        {
        }
    }
}