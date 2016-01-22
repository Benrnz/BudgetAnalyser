using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using Xunit;
using Xunit.Abstractions;

namespace MapperGeneration
{
    public class BudgetMapperGenerator : MapperGenerator
    {
        private const string NameSpace = "BudgetAnalyser.Engine.Budget.Data";
        public BudgetMapperGenerator(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GenerateBudgetCollectionToDto()
        {
            Act<BudgetCollectionDto, BudgetCollection>(NameSpace);
        }

        [Fact]
        public void GenerateBudgetModelToDto()
        {
            Act<BudgetModelDto, BudgetModel>(NameSpace);
        }

        [Fact]
        public void GenerateBudgetBucketToDto()
        {
            Act<BudgetBucketDto, BudgetBucket>(NameSpace);
        }
    }
}
