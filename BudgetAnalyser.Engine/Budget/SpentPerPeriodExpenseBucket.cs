namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A spent per period expense bucket.  This kind of bucket will not accumulate funds from period to period (month/fortnight). Any unspent
    ///     funds at the end of the period are transferred to surplus.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Budget.BillToPayExpenseBucket" />
    public class SpentPerPeriodExpenseBucket : BillToPayExpenseBucket
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SpentPerPeriodExpenseBucket" /> class.
        /// </summary>
        public SpentPerPeriodExpenseBucket()
        {
            // Default constructor required for deserialisation.
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpentPerPeriodExpenseBucket" /> class.
        /// </summary>
        public SpentPerPeriodExpenseBucket(string code, string name)
            : base(code, name)
        {
        }

        /// <summary>
        ///     Gets a description of this type of bucket. By default this is the <see cref="System.Type.Name" />
        /// </summary>
        public override string TypeDescription => "Spent Monthly/Fortnightly Expense";
    }
}