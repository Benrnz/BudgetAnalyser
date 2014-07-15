using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    // TODO if this works ok implement for all mappers to reduce the number of interfaces.
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetBucketDto, BudgetBucket>))]
    public class DtoToBudgetBucketMapper : BasicMapper<BudgetBucketDto, BudgetBucket>
    {
        private readonly IBudgetBucketFactory bucketFactory;

        public DtoToBudgetBucketMapper([NotNull] IBudgetBucketFactory bucketFactory)
        {
            if (bucketFactory == null)
            {
                throw new ArgumentNullException("bucketFactory");
            }

            this.bucketFactory = bucketFactory;
        }

        public override BudgetBucket Map([NotNull] BudgetBucketDto source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var bucket = this.bucketFactory.Build(source.Type);
            bucket.Code = source.Code;
            bucket.Description = source.Description;
            return bucket;
        }
    }
}