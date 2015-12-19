namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A expense bucket used to represent any expense classification.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Budget.BudgetBucket" />
    public abstract class ExpenseBucket : BudgetBucket
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpenseBucket" /> class.
        /// </summary>
        protected ExpenseBucket()
        {
            // Default constructor required for deserialisation.
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpenseBucket" /> class.
        /// </summary>
        protected ExpenseBucket(string code, string name)
            : base(code, name)
        {
        }
    }
}