namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A saved up for expense bucket.  This kind of bucket will accumulate funds from period to the next period (month/fortnight) if not spent
    ///     completely each month.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Budget.BillToPayExpenseBucket" />
    public class SavedUpForExpenseBucket : BillToPayExpenseBucket
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SavedUpForExpenseBucket" /> class.
        /// </summary>
        public SavedUpForExpenseBucket()
        {
            // Default constructor required for deserialisation.
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SavedUpForExpenseBucket" /> class.
        /// </summary>
        public SavedUpForExpenseBucket(string code, string name)
            : base(code, name)
        {
        }

        /// <summary>
        ///     Gets a description of this type of bucket. By default this is the <see cref="System.Type.Name" />
        /// </summary>
        public override string TypeDescription => "Accumulated Expense";
    }
}