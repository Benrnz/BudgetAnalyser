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
        public override BudgetModelDto Map([NotNull] BudgetModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            return new BudgetModelDto
            {
                EffectiveFrom = model.EffectiveFrom,
                Expenses = model.Expenses.Select(e => new ExpenseDto { Amount = e.Amount, BudgetBucketCode = e.Bucket.Code }).ToList(),
                Incomes = model.Incomes.Select(i => new IncomeDto { Amount = i.Amount, BudgetBucketCode = i.Bucket.Code }).ToList(),
                LastModified = model.LastModified,
                LastModifiedComment = model.LastModifiedComment,
                Name = model.Name,
            };
        }
    }
}