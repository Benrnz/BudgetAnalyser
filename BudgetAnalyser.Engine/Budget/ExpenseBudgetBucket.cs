namespace BudgetAnalyser.Engine.Budget
{
    public abstract class ExpenseBudgetBucket : BudgetBucket
    {
        protected ExpenseBudgetBucket()
        {
            // Default constructor required for deserialisation.
        }

        protected ExpenseBudgetBucket(string code, string name)
            : base(code, name)
        {
        }
    }
}