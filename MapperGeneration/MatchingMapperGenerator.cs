using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using Xunit;
using Xunit.Abstractions;

namespace MapperGeneration
{
    public class MatchingMapperGenerator : MapperGenerator
    {
        private const string NameSpace = "BudgetAnalyser.Engine.Matching.Data";
        public MatchingMapperGenerator(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GenerateMatchingRuleToDto()
        {
            Act<MatchingRuleDto, MatchingRule>(NameSpace);
        }
    }
}
