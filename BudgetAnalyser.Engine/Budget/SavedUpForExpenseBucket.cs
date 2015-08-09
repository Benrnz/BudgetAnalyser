namespace BudgetAnalyser.Engine.Budget
{
    public class SavedUpForExpenseBucket : BillToPayExpenseBucket
    {
        public SavedUpForExpenseBucket()
        {
            // Default constructor required for deserialisation.
        }

        public SavedUpForExpenseBucket(string code, string name)
            : base(code, name)
        {
        }

        public override string TypeDescription => "Accumulated Expense";
        
    }
}