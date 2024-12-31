using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    internal class AnzVisaStatementImporterV1TestHarness : AnzVisaStatementImporterV1
    {
        public AnzVisaStatementImporterV1TestHarness([NotNull] BankImportUtilities importUtilities, IReaderWriterSelector readerWriterSelector)
            : base(importUtilities, new FakeLogger(), readerWriterSelector)
        {
        }

        public Func<string, IEnumerable<string>> ReadLinesOverride { get; set; }
        public Func<string, string> ReadTextChunkOverride { get; set; }

        protected override Task<IEnumerable<string>> ReadLinesAsync(string fileName)
        {
            return ReadLinesOverride is null
                ? Task.FromResult((IEnumerable<string>)new List<string>())
                : Task.FromResult(ReadLinesOverride(fileName));
        }

        protected override Task<string> ReadTextChunkAsync(string filePath)
        {
            return ReadTextChunkOverride is null
                ? Task.FromResult("Card,Type,Amount,Details,TransactionDate,ProcessedDate,ForeignCurrencyAmount,ConversionCharge\r\n4323-****-****-1234,D,32.36,Z Queen Street          Auckland      Nz ,24/06/2014,25/06/2014,,\r\n")
                : Task.FromResult(ReadTextChunkOverride(filePath));
        }
    }
}
