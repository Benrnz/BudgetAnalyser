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
        private readonly ILogger logger;

        public DtoToBudgetModelMapper([NotNull] IBudgetBucketRepository bucketRepo, [NotNull] ILogger logger)
        {
            if (bucketRepo == null)
            {
                throw new ArgumentNullException("bucketRepo");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.bucketRepo = bucketRepo;
            this.logger = logger;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Prefered usage with IoC")]
        public override BudgetModel Map([NotNull] BudgetModelDto source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var model = new BudgetModel(this.logger)
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