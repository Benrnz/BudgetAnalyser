using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    public class DtoToBudgetModelMapper : BasicMapper<BudgetModelDto, BudgetModel>
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public DtoToBudgetModelMapper([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo == null)
            {
                throw new ArgumentNullException("bucketRepo");
            }

            this.bucketRepo = bucketRepo;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Prefered usage with IoC")]
        public override BudgetModel Map([NotNull] BudgetModelDto data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var model = new BudgetModel
            {
                EffectiveFrom = data.EffectiveFrom,
                LastModified = data.LastModified ?? DateTime.Now,
                LastModifiedComment = data.LastModifiedComment,
                Name = data.Name,
            };

            IEnumerable<Expense> expenses = data.Expenses.Select(dto => new Expense { Amount = dto.Amount, Bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode) });
            IEnumerable<Income> incomes = data.Incomes.Select(dto => new Income { Amount = dto.Amount, Bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode) });
            model.Update(incomes, expenses);
            return model;
        }
    }
}