using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    public class AnzVisaStatementImporterV1TestHarness : AnzVisaStatementImporterV1
    {
        public AnzVisaStatementImporterV1TestHarness([NotNull] BankImportUtilities importUtilities)
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
                return Task.FromResult("4323-****-****-1234,D,32.36,Z Queen Street          Auckland      Nz ,24/06/2014,25/06/2014,");
            }

            return Task.FromResult(ReadTextChunkOverride(filePath));
        }
    }
}