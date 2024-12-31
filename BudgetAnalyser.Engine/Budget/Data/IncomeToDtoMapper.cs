using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    internal partial class MapperIncomeDto2Income(IBudgetBucketRepository bucketRepo)
    {
        private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));

        partial void ToDtoPostprocessing(ref IncomeDto dto, Income model)
        {
            dto.BudgetBucketCode = model.Bucket.Code;
        }

        partial void ToModelPostprocessing(IncomeDto dto, ref Income model)
        {
            model.Bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode);
        }
    }
}
