using System;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC]
    public class DataBudgetCollectionToBudgetCollectionMapper
    {
        private readonly DataBudgetModelToBudgetModelMapper budgetModelMapper;

        public DataBudgetCollectionToBudgetCollectionMapper([NotNull] DataBudgetModelToBudgetModelMapper budgetModelMapper)
        {
            if (budgetModelMapper == null)
            {
                throw new ArgumentNullException("budgetModelMapper");
            }

            this.budgetModelMapper = budgetModelMapper;
        }

        public BudgetCollection Map(DataBudgetCollection budgetCollection)
        {
            var collection = new BudgetCollection(budgetCollection.Budgets.Select(d => this.budgetModelMapper.Map(d)))
            {
                FileName = budgetCollection.FileName,
            };

            return collection;
        }
    }
}
