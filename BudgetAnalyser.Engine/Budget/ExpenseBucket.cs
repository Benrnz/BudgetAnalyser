namespace BudgetAnalyser.Engine.Budget
{
    public abstract class ExpenseBucket : BudgetBucket
    {
        protected ExpenseBucket()
        {
            // Default constructor required for deserialisation.
        }

        protected ExpenseBucket(string code, string name)
            : base(code, name)
        {
        }
    }
}