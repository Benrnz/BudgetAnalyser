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
            Act<BudgetCollectionDto, BudgetCollection>("BudgetAnalyser.Engine.Budget.Data");
        }

        [Fact]
        public void GenerateBudgetModelToDto()
        {
            Act<BudgetModelDto, BudgetModel>("BudgetAnalyser.Engine.Budget.Data");
        }

        [Fact]
        public void GenerateBudgetBucketToDto()
        {
            Act<BudgetBucketDto, BudgetBucket>("BudgetAnalyser.Engine.Budget.Data");
        }
    }
}
