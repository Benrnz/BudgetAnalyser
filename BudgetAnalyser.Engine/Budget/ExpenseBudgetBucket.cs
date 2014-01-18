namespace BudgetAnalyser.Engine.Budget
{
    public abstract class ExpenseBudgetBucket : BudgetBucket
    {
        protected ExpenseBudgetBucket()
            : base()
        {
        }

        protected ExpenseBudgetBucket(string code, string name)
            : base(code, name)
        {
        }
    }
}