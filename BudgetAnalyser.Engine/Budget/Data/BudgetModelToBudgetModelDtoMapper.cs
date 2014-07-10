using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    public class BudgetModelToBudgetModelDtoMapper
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Prefered usage with IoC")]
        public BudgetModelDto Map([NotNull] BudgetModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            return new BudgetModelDto
            {
                EffectiveFrom = model.EffectiveFrom,
                Expenses = new List<Expense>(model.Expenses),
                Incomes = new List<Income>(model.Incomes),
                LastModified = model.LastModified,
                LastModifiedComment = model.LastModifiedComment,
                Name = model.Name,
            };
        }
    }
}