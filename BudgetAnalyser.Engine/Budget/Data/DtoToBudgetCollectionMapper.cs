using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetCollectionDto, BudgetCollection>))]
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
        public override BudgetCollection Map([NotNull] BudgetCollectionDto source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var collection = new BudgetCollection(source.Budgets.Select(d => this.budgetModelMapper.Map(d)))
            {
                FileName = source.FileName,
            };

            return collection;
        }
    }
}