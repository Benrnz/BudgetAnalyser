namespace BudgetAnalyser.Engine.Budget
{
    public abstract class BillToPayExpenseBucket : ExpenseBucket
    {
        protected BillToPayExpenseBucket()
        {
            // Default constructor required for deserialisation.
        }

        protected BillToPayExpenseBucket(string code, string name)
            : base(code, name)
        {
        }
    }
}