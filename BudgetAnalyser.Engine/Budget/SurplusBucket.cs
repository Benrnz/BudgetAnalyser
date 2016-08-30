namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     The surplus budget bucket.  There can be only one Surplus bucket.  This is a special system bucket that contains
    ///     any left over funds after filling all the other buckets.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Budget.BudgetBucket" />
    public class SurplusBucket : BudgetBucket
    {
        /// <summary>
        ///     The constant surplus bucket code.
        /// </summary>
        public const string SurplusCode = "SURPLUS";

        /// <summary>
        ///     The constant for surplus description.
        /// </summary>
        public const string SurplusDescription = "A special bucket to allocate against any discretionary spending.";

        /// <summary>
        ///     Initializes a new instance of the <see cref="SurplusBucket" /> class.
        /// </summary>
        public SurplusBucket()
            : base(SurplusCode, SurplusDescription)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SurplusBucket" /> class.
        /// </summary>
        protected SurplusBucket(string bucketCode, string description) : base(bucketCode, description)
        {
        }

        /// <summary>
        ///     Gets a description of this type of bucket. By default this is the <see cref="System.Type.Name" />
        /// </summary>
        public override string TypeDescription => "Calculated Surplus";
    }
}