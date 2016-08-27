namespace BudgetAnalyser.Engine.Mobile
{
    /// <summary>
    ///     A DTO type to export information about one ledger bucket
    /// </summary>
    public class SummarisedLedgerBucket
    {
        /// <summary>
        ///     The bucket code for the ledger
        /// </summary>
        public string BucketCode { get; set; }

        /// <summary>
        ///     The type (behaviour) of the bucket. IE: Spent Monthly or Saved.
        /// </summary>
        public string BucketType { get; set; }

        /// <summary>
        ///     The bucket description from the budget
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     The monthly budget amount credited into this ledger
        /// </summary>
        public decimal MonthlyBudgetAmount { get; set; }

        /// <summary>
        ///     The opening balance at the begining of the month
        /// </summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>
        ///     The funds remaining in the bucket
        /// </summary>
        public decimal RemainingBalance { get; set; }
    }
}