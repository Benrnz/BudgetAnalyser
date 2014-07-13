using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    // TODO if this works ok implement for all mappers to reduce the number of interfaces.
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<BudgetBucketDto, BudgetBucket>))]
    public class BudgetBucketDtoToBudgetBucketMapper : BasicMapper<BudgetBucketDto, BudgetBucket>
    {
        private readonly BudgetBucketFactory bucketFactory;

        public BudgetBucketDtoToBudgetBucketMapper([NotNull] BudgetBucketFactory bucketFactory)
        {
            if (bucketFactory == null)
            {
                throw new ArgumentNullException("bucketFactory");
            }

            this.bucketFactory = bucketFactory;
        }

        public override BudgetBucket Map(BudgetBucketDto dto)
        {
            var bucket = this.bucketFactory.Build(dto.Type);
            bucket.Code = dto.Code;
            bucket.Description = dto.Description;
            return bucket;
        }
    }
}