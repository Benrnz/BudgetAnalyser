using System;

namespace BudgetAnalyser.Engine.Budget.Data
{
    /// <summary>
    ///     A Dto object for persisting a Fixed Budget Bucket.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Budget.Data.BudgetBucketDto" />
    public class FixedBudgetBucketDto : BudgetBucketDto
    {
        /// <summary>
        ///     Gets or sets the created date.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        ///     Gets or sets the fixed budget amount.
        /// </summary>
        public decimal FixedBudgetAmount { get; set; }

        /// <summary>
        ///     Gets or sets the simple persistence bucket type.
        /// </summary>
        public override BucketDtoType Type
        {
            get => BucketDtoType.FixedBudgetProject;
            set { }
        }
    }
}
