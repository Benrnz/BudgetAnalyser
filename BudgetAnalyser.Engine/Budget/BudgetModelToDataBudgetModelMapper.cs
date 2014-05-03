using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC]
    public class BudgetModelToDataBudgetModelMapper
    {
        public virtual DataBudgetModel Map(BudgetModel model)
        {
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
