using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    internal class WestpacAccountStatementImporterV1TestHarness : WestpacAccountStatementImporterV1
    {
        public WestpacAccountStatementImporterV1TestHarness([NotNull] BankImportUtilities importUtilities, IReaderWriterSelector readerWriterSelector)
            : base(importUtilities, new FakeLogger(), readerWriterSelector)
        {
        }

        public Func<string, IEnumerable<string>> ReadLinesOverride { get; set; }
        public Func<string, string> ReadTextChunkOverride { get; set; }

        protected override Task<IEnumerable<string>> ReadLinesAsync(string fileName)
        {
            if (ReadLinesOverride is null)
            {
                return Task.FromResult((IEnumerable<string>)new string[] { });
            }

            return Task.FromResult(ReadLinesOverride(fileName));
        }

        protected override Task<string> ReadTextChunkAsync(string filePath)
        {
            if (ReadTextChunkOverride is null)
            {
                return Task.FromResult("Date,Amount,Other Party,Description,Reference,Particulars,Analysis Code\r\n20/07/2020,-12.50,\"Brew On Quay\",\"EFTPOS TRANSACTION\",\"20-16:10-941\",\"************\",\"7786 30941\"");
            }

            return Task.FromResult(ReadTextChunkOverride(filePath));
        }
    }
}