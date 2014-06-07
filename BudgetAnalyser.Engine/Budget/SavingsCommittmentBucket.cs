namespace BudgetAnalyser.Engine.Budget
{
    public class SavingsCommittmentBucket : ExpenseBucket
    {
        public SavingsCommittmentBucket()
        {
            // Default constructor required for deserialisation.
        }

        public SavingsCommittmentBucket(string code, string name)
            : base(code, name)
        {
        }

        public override string TypeDescription
        {
            get { return "Savings Committment Monthly Expense"; }
        }
    }
}