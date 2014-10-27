using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class CsvOnDiskStatementModelRepositoryV1TestHarness : CsvOnDiskStatementModelRepositoryV1
    {
        public CsvOnDiskStatementModelRepositoryV1TestHarness(IUserMessageBox userMessageBox, BankImportUtilities importUtilities)
            : base(userMessageBox, 
                    importUtilities, 
                    new FakeLogger(), 
                    new TransactionSetDtoToStatementModelMapper(), 
                    new StatementModelToTransactionSetDtoMapper())
        {
        }

        public CsvOnDiskStatementModelRepositoryV1TestHarness(
            IUserMessageBox userMessageBox, 
            BankImportUtilities importUtilities, 
            BasicMapper<TransactionSetDto, StatementModel> toDomainMapper, 
            BasicMapper<StatementModel, TransactionSetDto> toDtoMapper)
            : base(userMessageBox,
                    importUtilities,
                    new FakeLogger(),
                    toDomainMapper,
                    toDtoMapper)
        {
        }

        public Func<string, IEnumerable<string>> ReadLinesOverride { get; set; }

        protected override Task<IEnumerable<string>> ReadLinesAsync(string fileName)
        {
            if (ReadLinesOverride == null)
            {
                return Task.FromResult<IEnumerable<string>>(new List<string>());
            }

            return Task.FromResult(ReadLinesOverride(fileName));
        }

        protected override Task<IEnumerable<string>> ReadLinesAsync(string fileName, int lines)
        {
            if (ReadLinesOverride == null)
            {
                return Task.FromResult<IEnumerable<string>>(new List<string>());
            }

            return Task.FromResult(ReadLinesOverride(fileName).Take(lines));
        }
    }
}