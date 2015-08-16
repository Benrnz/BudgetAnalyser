using System;

namespace BudgetAnalyser.Engine.Budget.Data
{
    public class FixedBudgetBucketDto : BudgetBucketDto
    {
        public DateTime Created { get; set; }
        public decimal FixedBudgetAmount { get; set; }

        public override BucketDtoType Type
        {
            get { return BucketDtoType.FixedBudgetProject; }
            set { }
        }
    }
}