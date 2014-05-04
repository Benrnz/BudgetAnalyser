using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC]
    public class BudgetModelToDataBudgetModelMapper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Prefered usage with IoC")]
        public DataBudgetModel Map([NotNull] BudgetModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            return new DataBudgetModel
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
