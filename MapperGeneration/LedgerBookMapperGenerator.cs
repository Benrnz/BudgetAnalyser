using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using Xunit;
using Xunit.Abstractions;

namespace MapperGeneration
{
    public class LedgerBookMapperGenerator : MapperGenerator
    {
        private const string NameSpace = "BudgetAnalyser.Engine.Ledger.Data";
        public LedgerBookMapperGenerator(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void GenerateStatementModelToDto()
        {
            Act<LedgerBookDto, LedgerBook>(NameSpace);
        }
    }
}
