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
                LastModified = data.LastModified,
                LastModifiedComment = data.LastModifiedComment,
                Name = data.Name,
            };

            // TODO is this really necessary? Could just make the lists internal and set them.
            var expenses = new List<Expense>(data.Expenses);
            var incomes = new List<Income>(data.Incomes);
            model.Update(incomes, expenses);
            return model;
        }
    }
}
