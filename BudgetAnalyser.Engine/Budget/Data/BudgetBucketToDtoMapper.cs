using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetBucket, BudgetBucketDto>))]
    public class BudgetBucketToDtoMapper : BasicMapper<BudgetBucket, BudgetBucketDto>
    {
        private readonly IBudgetBucketFactory bucketFactory;

        public BudgetBucketToDtoMapper([NotNull] IBudgetBucketFactory bucketFactory)
        {
            if (bucketFactory == null)
            {
                throw new ArgumentNullException("bucketFactory");
            }

            this.bucketFactory = bucketFactory;
        }

        public override BudgetBucketDto Map([NotNull] BudgetBucket source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new BudgetBucketDto
            {
                Code = source.Code,
                Description = source.Description,
                Type = this.bucketFactory.SerialiseType(source),
            };
        }
    }
}