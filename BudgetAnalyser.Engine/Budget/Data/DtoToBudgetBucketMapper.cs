using System;
using AutoMapper;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetBucketDto, BudgetBucket>))]
    public class DtoToBudgetBucketMapper : MagicMapper<BudgetBucketDto, BudgetBucket>
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

        public override BudgetBucket Map(BudgetBucketDto source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return Mapper.Map<BudgetBucket>(source, options => options.ConstructServicesUsing(type => this.bucketFactory.Build(source.Type)));
        }
    }
}