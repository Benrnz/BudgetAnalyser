using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    internal partial class Mapper_ExpenseDto_Expense
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public Mapper_ExpenseDto_Expense([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            this.bucketRepo = bucketRepo;
        }

        partial void ToModelPostprocessing(ExpenseDto dto, ref Expense model)
        {
            model.Bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode);
        }

        partial void ToDtoPostprocessing(ref ExpenseDto dto, Expense model)
        {
            dto.BudgetBucketCode = model.Bucket.Code;
        }
    }
}
