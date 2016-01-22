using BudgetAnalyser.Engine.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace MapperGeneration
{
    public class ApplicationDatabaseMapperGenerator : MapperGenerator
    {
        private const string NameSpace = "BudgetAnalyser.Engine.Persistence";
        public ApplicationDatabaseMapperGenerator(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GenerateApplicationDatabaseToDto()
        {
            Act<BudgetAnalyserStorageRoot, ApplicationDatabase>(NameSpace);
        }
    }
}
