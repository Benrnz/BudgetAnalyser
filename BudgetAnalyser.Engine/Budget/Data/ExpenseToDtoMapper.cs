using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    internal partial class MapperExpenseDto2Expense(IBudgetBucketRepository bucketRepo)
    {
        private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));

        partial void ToDtoPostprocessing(ref ExpenseDto dto, Expense model)
        {
            dto.BudgetBucketCode = model.Bucket.Code;
        }

        partial void ToModelPostprocessing(ExpenseDto dto, ref Expense model)
        {
            model.Bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode);
        }
    }
}
