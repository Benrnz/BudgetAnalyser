using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Statement
{
    [TestClass]
    public class AnzVisaStatementImporterV1Test
    {
        private BankImportUtilitiesTestHarness BankImportUtilities { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullBankImportUtilities()
        {
            new AnzVisaStatementImporterV1(null, new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullLogger()
        {
            new AnzVisaStatementImporterV1(new BankImportUtilities(new FakeLogger()), null);
            Assert.Fail();
        }

        [TestMethod]
        public async Task LoadShouldParseAFileWithExtraColumns()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = f => AnzVisaCsvTestData.TestData2();
            StatementModel result = await subject.LoadAsync("foo.bar", StatementModelTestData.VisaAccount);

            Assert.AreEqual(1, result.DurationInMonths);
            Assert.AreEqual(13, result.AllTransactions.Count());
        }

        [TestMethod]
        public async Task LoadShouldParseAGoodFile()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = f => AnzVisaCsvTestData.TestData1();
            StatementModel result = await subject.LoadAsync("foo.bar", StatementModelTestData.VisaAccount);

            Assert.AreEqual(1, result.DurationInMonths);
            Assert.AreEqual(13, result.AllTransactions.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task LoadShouldThrowGivenBadData()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = filename => AnzChequeCsvTestData.BadTestData1();
            await subject.LoadAsync("foo.bar", StatementModelTestData.VisaAccount);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowIfFileNotFound()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            BankImportUtilities.AbortIfFileDoesntExistOverride = s => { throw new FileNotFoundException(); };
            await subject.LoadAsync("foo.bar", StatementModelTestData.VisaAccount);
            Assert.Fail();
        }

        [TestMethod]
        public async Task TatseTestShouldReturnFalseGivenABadFile()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadTextChunkOverride = file => "lkjpoisjg809wutwuoipsahf98qyfg0w9ashgpiosxnhbvoiyxcu8o9ui9paso,spotiw93th98sh8,35345345,353453534521,lkhsldhlsk,shgjkshj,sgsjdgsd";
            bool result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TatseTestShouldReturnFalseGivenAnEmptyTasteTestResponse()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadTextChunkOverride = file => string.Empty;
            bool result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TatseTestShouldReturnFalseGivenNullTasteTestResponse()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadTextChunkOverride = file => null;
            bool result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TatseTestShouldReturnFalseGivenTheChequeFormat()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadTextChunkOverride = file => "Atm Debit,Anz  1234567 Queen St,Anz  S3A1234,Queen St,Anch  123456,-80.00,16/06/2014,"; // Cheque format given to Visa parser
            bool result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TatseTestShouldReturnTrueGivenAGoodFile()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            bool result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsTrue(result);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            BankImportUtilities = new BankImportUtilitiesTestHarness();
        }

        private AnzVisaStatementImporterV1TestHarness Arrange()
        {
            return new AnzVisaStatementImporterV1TestHarness(BankImportUtilities);
        }
    }
}