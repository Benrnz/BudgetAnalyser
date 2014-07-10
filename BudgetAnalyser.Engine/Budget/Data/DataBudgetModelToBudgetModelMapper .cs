using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    public class BudgetModelDtoToBudgetModelMapper
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Prefered usage with IoC")]
        public BudgetModel Map([NotNull] BudgetModelDto data)
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

            var expenses = new List<Expense>(data.Expenses);
            var incomes = new List<Income>(data.Incomes);
            model.Update(incomes, expenses);
            return model;
        }
    }
}