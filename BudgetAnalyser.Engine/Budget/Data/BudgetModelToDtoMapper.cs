using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetModel, BudgetModelDto>))]
    public class BudgetModelToDtoMapper : BasicMapper<BudgetModel, BudgetModelDto>
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Prefered usage with IoC")]
        public override BudgetModelDto Map([NotNull] BudgetModel source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new BudgetModelDto
            {
                EffectiveFrom = source.EffectiveFrom,
                Expenses = source.Expenses.Select(e => new ExpenseDto { Amount = e.Amount, BudgetBucketCode = e.Bucket.Code }).ToList(),
                Incomes = source.Incomes.Select(i => new IncomeDto { Amount = i.Amount, BudgetBucketCode = i.Bucket.Code }).ToList(),
                LastModified = source.LastModified,
                LastModifiedComment = source.LastModifiedComment,
                Name = source.Name,
            };
        }
    }
}