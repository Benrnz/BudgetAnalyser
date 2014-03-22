namespace BudgetAnalyser.Engine.Budget
{
    public class IncomeBudgetBucket : BudgetBucket
    {
        public IncomeBudgetBucket()
        {
            // Default constructor required for deserialisation.
        }

        public IncomeBudgetBucket(string code, string name) : base(code, name)
        {
        }

        public override string ToString()
        {
            return "Income: " + Description;
        }
    }
}