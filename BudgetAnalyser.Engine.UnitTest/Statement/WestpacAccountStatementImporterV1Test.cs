using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Statement
{
    [TestClass]
    public class WestpacAccountStatementImporterV1Test
    {
        private Mock<IReaderWriterSelector> mockReaderWriterSelector;
        private BankImportUtilitiesTestHarness BankImportUtilities { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullBankImportUtilities()
        {
            new WestpacAccountStatementImporterV1(null, new FakeLogger(), this.mockReaderWriterSelector.Object);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullLogger()
        {
            new WestpacAccountStatementImporterV1(new BankImportUtilities(new FakeLogger()), null, this.mockReaderWriterSelector.Object);
            Assert.Fail();
        }

        [TestMethod]
        public async Task LoadShouldParseAFileWithExtraColumns()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = f => WestpacChequeCsvTestData.TestData2();
            var result = await subject.LoadAsync("foo.bar", StatementModelTestData.ChequeAccount);

            Assert.AreEqual(1, result.DurationInMonths);
            Assert.AreEqual(7, result.AllTransactions.Count());
        }

        [TestMethod]
        public async Task LoadShouldParseAGoodFile()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = f => WestpacChequeCsvTestData.TestData1();
            var result = await subject.LoadAsync("foo.bar", StatementModelTestData.ChequeAccount);

            Assert.AreEqual(1, result.DurationInMonths);
            Assert.AreEqual(7, result.AllTransactions.Count());
        }

        [TestMethod]
        public async Task LoadShouldParseAGoodFileAndOutputIt()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = f => WestpacChequeCsvTestData.TestData1();
            var result = await subject.LoadAsync("foo.bar", StatementModelTestData.ChequeAccount);

            Console.WriteLine("Date       Type             Description    Amount    ");
            foreach(var txn in result.AllTransactions)
            {
                Console.WriteLine($"{txn.Date:dd-MMM-yy} {txn.TransactionType.ToString().Truncate(10).PadRight(10)} {txn.Description.Truncate(12).PadRight(12)} {txn.Amount,10}");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public async Task LoadShouldThrowGivenBadData()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = filename => WestpacChequeCsvTestData.BadTestData1();
            await subject.LoadAsync("foo.bar", StatementModelTestData.ChequeAccount);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowIfFileNotFound()
        {
            var subject = Arrange();
            BankImportUtilities.AbortIfFileDoesntExistOverride = s => { throw new FileNotFoundException(); };
            await subject.LoadAsync("foo.bar", StatementModelTestData.ChequeAccount);
            Assert.Fail();
        }

        [TestMethod]
        public async Task TatseTestShouldReturnFalseGivenABadFile()
        {
            var subject = Arrange();
            // @"20/07/2020,25.26,""Acme Inc"",""DIRECT CREDIT"",""IFT012345667"",""IFT01234566"",""00444565652",
            subject.ReadTextChunkOverride = file => "lkjpoisjg809wutwuoipsahf98qyfg0w9ashgpiosxnhbvoiyxcu8o9ui9paso,spotiw93th98sh8,35345345,353453534521,lkhsldhlsk,shgjkshj,sgsjdgsd";
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TatseTestShouldReturnFalseGivenAnEmptyTasteTestResponse()
        {
            var subject = Arrange();
            subject.ReadTextChunkOverride = file => string.Empty;
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TatseTestShouldReturnFalseGivenNullTasteTestResponse()
        {
            var subject = Arrange();
            subject.ReadTextChunkOverride = file => null;
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TatseTestShouldReturnFalseGivenTheAnzVisaFormat()
        {
            var subject = Arrange();
            subject.ReadTextChunkOverride = file => "4367-****-****-1234,D,32.36,Z Quay Street          Auckland      Nz ,24/06/2014,25/06/2014,"; // Visa format given to Westpac parser
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TatseTestShouldReturnFalseGivenTheAnzChequeFormat()
        {
            var subject = Arrange();
            subject.ReadTextChunkOverride = file => "Payment,Acme Inc,Acme LLB Inc,Smith Vj,1671190,-23.40,19/06/2014,"; // Anz format given to Westpac parser
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TatseTestShouldReturnTrueGivenAGoodFile()
        {
            var subject = Arrange();
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsTrue(result);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            BankImportUtilities = new BankImportUtilitiesTestHarness();
            this.mockReaderWriterSelector = new Mock<IReaderWriterSelector>();
        }

        private WestpacAccountStatementImporterV1TestHarness Arrange()
        {
            return new WestpacAccountStatementImporterV1TestHarness(BankImportUtilities, this.mockReaderWriterSelector.Object);
        }
    }
}