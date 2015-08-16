using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class AnzAccountStatementImporterV1TestHarness : AnzAccountStatementImporterV1
    {
        public AnzAccountStatementImporterV1TestHarness([NotNull] BankImportUtilities importUtilities)
            : base(importUtilities, new FakeLogger())
        {
        }

        public Func<string, IEnumerable<string>> ReadLinesOverride { get; set; }
        public Func<string, string> ReadTextChunkOverride { get; set; }

        protected override Task<IEnumerable<string>> ReadLinesAsync(string fileName)
        {
            if (ReadLinesOverride == null)
            {
                return Task.FromResult((IEnumerable<string>)new List<string>());
            }

            return Task.FromResult(ReadLinesOverride(fileName));
        }

        protected override Task<string> ReadTextChunkAsync(string filePath)
        {
            if (ReadTextChunkOverride == null)
            {
                return Task.FromResult("Atm Debit,Anz  1234567 Queen St,Anz  S3A1234,Queen St,Anch  123456,-80.00,16/06/2014,");
            }

            return Task.FromResult(ReadTextChunkOverride(filePath));
        }
    }
}