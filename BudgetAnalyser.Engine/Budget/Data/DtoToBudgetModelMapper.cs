using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetModelDto, BudgetModel>))]
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
        public override BudgetModel Map([NotNull] BudgetModelDto source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var model = new BudgetModel
            {
                EffectiveFrom = source.EffectiveFrom,
                LastModified = source.LastModified ?? DateTime.Now,
                LastModifiedComment = source.LastModifiedComment,
                Name = source.Name,
            };

            IEnumerable<Expense> expenses = source.Expenses.Select(dto => new Expense { Amount = dto.Amount, Bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode) });
            IEnumerable<Income> incomes = source.Incomes.Select(dto => new Income { Amount = dto.Amount, Bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode) });
            model.Update(incomes, expenses);
            return model;
        }
    }
}