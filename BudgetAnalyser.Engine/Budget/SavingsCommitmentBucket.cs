namespace BudgetAnalyser.Engine.Budget
{
    public class SavingsCommitmentBucket : ExpenseBucket
    {
        public SavingsCommitmentBucket()
        {
            // Default constructor required for deserialisation.
        }

        public SavingsCommitmentBucket(string code, string name)
            : base(code, name)
        {
        }

        public override string TypeDescription => "Savings Commitment Monthly Expense";
    }
}