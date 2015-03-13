using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class AnzVisaStatementImporterV1TestHarness : AnzVisaStatementImporterV1
    {
        public AnzVisaStatementImporterV1TestHarness([NotNull] IUserMessageBox userMessageBox, [NotNull] BankImportUtilities importUtilities)
            : base(importUtilities, new FakeLogger())
        {
        }

        public Func<string, IEnumerable<string>> ReadLinesOverride { get; set; }

        public Func<string, string> ReadTextChunkOverride { get; set; }

        protected override IEnumerable<string> ReadLines(string fileName)
        {
            if (ReadLinesOverride == null)
            {
                return new List<string>();
            }

            return ReadLinesOverride(fileName);
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