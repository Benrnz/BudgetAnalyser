using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC]
    public class DataBudgetModelToBudgetModelMapper
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Prefered usage with IoC")]
        public BudgetModel Map([NotNull] DataBudgetModel data)
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