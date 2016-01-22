using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using Xunit;
using Xunit.Abstractions;

namespace MapperGeneration
{
    public class StatementMapperGenerator : MapperGenerator
    {
        private const string NameSpace = "BudgetAnalyser.Engine.Statement.Data";
        public StatementMapperGenerator(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GenerateStatementModelToDto()
        {
            Act<TransactionSetDto, StatementModel>(NameSpace);
        }
    }
}
