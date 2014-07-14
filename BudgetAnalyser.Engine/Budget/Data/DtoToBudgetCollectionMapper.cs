using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    public class DtoToBudgetCollectionMapper : BasicMapper<BudgetCollectionDto, BudgetCollection>
    {
        private readonly DtoToBudgetModelMapper budgetModelMapper;

        public DtoToBudgetCollectionMapper([NotNull] DtoToBudgetModelMapper budgetModelMapper)
        {
            if (budgetModelMapper == null)
            {
                throw new ArgumentNullException("budgetModelMapper");
            }

            this.budgetModelMapper = budgetModelMapper;
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom Collection")]
        public override BudgetCollection Map([NotNull] BudgetCollectionDto budgetCollectionDto)
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