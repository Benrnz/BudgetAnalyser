using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    internal partial class MapperBudgetBucketDtoBudgetBucket
    {
        private readonly IBudgetBucketFactory bucketFactory;

        public MapperBudgetBucketDtoBudgetBucket([NotNull] IBudgetBucketFactory bucketFactory)
        {
            if (bucketFactory is null)
            {
                throw new ArgumentNullException(nameof(bucketFactory));
            }

            this.bucketFactory = bucketFactory;
        }

        // ReSharper disable once RedundantAssignment
        partial void DtoFactory(ref BudgetBucketDto dto, BudgetBucket model)
        {
            dto = this.bucketFactory.BuildDto(model);
        }

        // ReSharper disable once RedundantAssignment
        partial void ModelFactory(BudgetBucketDto dto, ref BudgetBucket model)
        {
            model = this.bucketFactory.BuildModel(dto);
        }
    }
}
