using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    public class BudgetCollectionDtoToBudgetCollectionMapper
    {
        private readonly BudgetModelDtoToBudgetModelMapper budgetModelMapper;

        public BudgetCollectionDtoToBudgetCollectionMapper([NotNull] BudgetModelDtoToBudgetModelMapper budgetModelMapper)
        {
            if (budgetModelMapper == null)
            {
                throw new ArgumentNullException("budgetModelMapper");
            }

            this.budgetModelMapper = budgetModelMapper;
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom Collection")]
        public BudgetCollection Map([NotNull] BudgetCollectionDto budgetCollectionDto)
        {
            if (budgetCollectionDto == null)
            {
                throw new ArgumentNullException("budgetCollectionDto");
            }

            var collection = new BudgetCollection(budgetCollectionDto.Budgets.Select(d => this.budgetModelMapper.Map(d)))
            {
                FileName = budgetCollectionDto.FileName,
            };

            return collection;
        }
    }
}