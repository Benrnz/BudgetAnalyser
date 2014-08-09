using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override IEnumerable<string> ReadLines(string fileName)
        {
            if (ReadLinesOverride == null)
            {
                return new List<string>();
            }

            return ReadLinesOverride(fileName);
        }

        protected override IEnumerable<string> ReadLines(string fileName, int lines)
        {
            if (ReadLinesOverride == null)
            {
                return new List<string>();
            }

            return ReadLinesOverride(fileName).Take(lines);
        }
    }
}