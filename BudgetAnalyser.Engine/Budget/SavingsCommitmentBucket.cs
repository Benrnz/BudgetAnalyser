namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A savings commitment bucket.  This bucket represents a commitment to save a certain amount each month/fortnight.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Budget.ExpenseBucket" />
    public class SavingsCommitmentBucket : ExpenseBucket
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SavingsCommitmentBucket" /> class.
        /// </summary>
        public SavingsCommitmentBucket()
        {
            // Default constructor required for deserialisation.
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SavingsCommitmentBucket" /> class.
        /// </summary>
        public SavingsCommitmentBucket(string code, string name)
            : base(code, name)
        {
        }

        /// <summary>
        ///     Gets a description of this type of bucket. By default this is the <see cref="System.Type.Name" />
        /// </summary>
        public override string TypeDescription => "Savings Commitment Monthly Expense";
    }
}