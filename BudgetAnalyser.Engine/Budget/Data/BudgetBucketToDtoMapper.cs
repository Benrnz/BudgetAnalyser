using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    internal partial class Mapper_BudgetBucketDto_BudgetBucket
    {
        private readonly IBudgetBucketFactory bucketFactory;

        public Mapper_BudgetBucketDto_BudgetBucket([NotNull] IBudgetBucketFactory bucketFactory)
        {
            if (bucketFactory == null) throw new ArgumentNullException(nameof(bucketFactory));
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