namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     Represents a bill to pay bucket.  Any subclass of this will represent an expense in the budget as well as being
    ///     used to classify transactions.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Budget.ExpenseBucket" />
    public abstract class BillToPayExpenseBucket : ExpenseBucket
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BillToPayExpenseBucket" /> class.
        /// </summary>
        protected BillToPayExpenseBucket()
        {
            // Default constructor required for deserialisation.
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BillToPayExpenseBucket" /> class.
        /// </summary>
        protected BillToPayExpenseBucket(string code, string name)
            : base(code, name)
        {
        }
    }
}