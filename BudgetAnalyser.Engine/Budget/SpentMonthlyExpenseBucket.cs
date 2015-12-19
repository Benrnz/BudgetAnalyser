namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A spent monthly expense bucket.  This kind of bucket will not accumulate funds from month to month. Any unspent
    ///     funds at the end of the month are transfered to surplus.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Budget.BillToPayExpenseBucket" />
    public class SpentMonthlyExpenseBucket : BillToPayExpenseBucket
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SpentMonthlyExpenseBucket" /> class.
        /// </summary>
        public SpentMonthlyExpenseBucket()
        {
            // Default constructor required for deserialisation.
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpentMonthlyExpenseBucket" /> class.
        /// </summary>
        public SpentMonthlyExpenseBucket(string code, string name)
            : base(code, name)
        {
        }

        /// <summary>
        ///     Gets a description of this type of bucket. By default this is the <see cref="System.Type.Name" />
        /// </summary>
        public override string TypeDescription => "Spent Monthly Expense";
    }
}