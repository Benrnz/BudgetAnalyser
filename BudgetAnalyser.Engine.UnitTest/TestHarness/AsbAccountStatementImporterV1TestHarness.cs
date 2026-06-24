using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    internal class AsbAccountStatementImporterV1TestHarness : AsbAccountStatementImporterV1
    {
        public AsbAccountStatementImporterV1TestHarness(BankImportUtilities importUtilities, IReaderWriterSelector readerWriterSelector)
            : base(importUtilities, new FakeLogger(), readerWriterSelector)
        {
        }

        public Func<string, IEnumerable<string>> ReadLinesOverride { get; set; }
        public Func<string, string> ReadTextChunkOverride { get; set; }

        protected override Task<IEnumerable<string>> ReadLinesAsync(string fileName)
        {
            return ReadLinesOverride is null
                ? Task.FromResult((IEnumerable<string>)[])
                : Task.FromResult(ReadLinesOverride(fileName));
        }

        protected override Task<string> ReadTextChunkAsync(string filePath)
        {
            return ReadTextChunkOverride is null
                ? Task.FromResult("Created date / time : 24 June 2026 / 10:48:59\r\nBank 12; Branch 3638; Account 0123486-00 (Everyday)\r\nFrom date 20260604\r\nTo date 20260624\r\nAvail Bal : 68.76 as of 20260620\r\nLedger Balance : 68.76 as of 20260624\r\nDate,Unique Id,Tran Type,Cheque Number,Payee,Memo,Amount\r\n\r\n19/06/2026,2026061901,EFTPOS,,BH INDIAN CUSINE AUCKLAND,EFTPOS,-26.52")
                : Task.FromResult(ReadTextChunkOverride(filePath));
        }
    }
}
