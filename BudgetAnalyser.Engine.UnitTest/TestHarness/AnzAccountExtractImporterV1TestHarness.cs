using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Transactions;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    internal class AnzAccountExtractImporterV1TestHarness : AnzAccountExtractImporterV1
    {
        public AnzAccountExtractImporterV1TestHarness([NotNull] BankImportUtilities importUtilities, IReaderWriterSelector readerWriterSelector)
            : base(importUtilities, new FakeLogger(), readerWriterSelector)
        {
        }

        public Func<string, IEnumerable<string>> ReadLinesOverride { get; set; }
        public Func<string, string> ReadTextChunkOverride { get; set; }

        protected override Task<IEnumerable<string>> ReadLinesAsync(string fileName)
        {
            return ReadLinesOverride is null
                ? Task.FromResult((IEnumerable<string>)new string[] { })
                : Task.FromResult(ReadLinesOverride(fileName));
        }

        protected override Task<string> ReadTextChunkAsync(string filePath)
        {
            return ReadTextChunkOverride is null
                ? Task.FromResult("Type,Details,Particulars,Code,Reference,Amount,Date,ForeignCurrencyAmount,ConversionCharge\r\nAtm Debit,Anz  1234567 Queen St,Anz  S3A1234,Queen St,Anch  123456,-80.00,16/06/2014,,")
                : Task.FromResult(ReadTextChunkOverride(filePath));
        }
    }
}
