namespace BudgetAnalyser.Engine.Budget
{
    public class SavedUpForExpense : ExpenseBudgetBucket
    {
        public SavedUpForExpense(string code, string name)
            : base(code, name)
        {
        }

        public override string TypeDescription
        {
            get { return "Accumulated Expense"; }
        }
    }
}