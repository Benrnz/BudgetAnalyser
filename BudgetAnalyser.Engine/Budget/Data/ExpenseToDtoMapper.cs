using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    internal partial class Mapper_ExpenseDto_Expense
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public Mapper_ExpenseDto_Expense([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo is null) throw new ArgumentNullException(nameof(bucketRepo));
            this.bucketRepo = bucketRepo;
        }

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