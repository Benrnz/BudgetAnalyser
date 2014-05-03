using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC]
    public class DataBudgetModelToBudgetModelMapper
    {
        public virtual BudgetModel Map(DataBudgetModel data)
        {
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
