using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<BudgetBucket, BudgetBucketDto>))]
    public class BudgetBucketToBudgetBucketDtoMapper : BasicMapper<BudgetBucket, BudgetBucketDto>
    {
        private readonly BudgetBucketFactory bucketFactory;

        public BudgetBucketToBudgetBucketDtoMapper([NotNull] BudgetBucketFactory bucketFactory)
        {
            if (bucketFactory == null)
            {
                throw new ArgumentNullException("bucketFactory");
            }

            this.bucketFactory = bucketFactory;
        }

        public override BudgetBucketDto Map(BudgetBucket bucket)
        {
            return new BudgetBucketDto
            {
                Code = bucket.Code,
                Description = bucket.Description,
                Type = this.bucketFactory.SerialiseType(bucket),
            };
        }
    }
}