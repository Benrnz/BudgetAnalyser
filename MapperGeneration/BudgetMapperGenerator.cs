using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using Xunit;
using Xunit.Abstractions;

namespace MapperGeneration
{
    public class BudgetMapperGenerator : MapperGenerator
    {

        public BudgetMapperGenerator(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GenerateBudgetCollectionToDto()
        {
            Act<BudgetCollectionDto, BudgetCollection>();
        }

        [Fact]
        public void GenerateBudgetToDto()
        {
            Act<BudgetModelDto, BudgetModel>();
        }
    }
}
