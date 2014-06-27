using System;
using System.IO;
using System.Linq;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Statement
{
    [TestClass]
    public class AnzVisaStatementImporterV1Test
    {
        private BankImportUtilitiesTestHarness BankImportUtilities { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullBankImportUtilities()
        {
            new AnzVisaStatementImporterV1(new FakeUserMessageBox(), null, new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullLogger()
        {
            new AnzVisaStatementImporterV1(new FakeUserMessageBox(), new BankImportUtilities(new FakeLogger()), null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullMessageBox()
        {
            new AnzVisaStatementImporterV1(null, new BankImportUtilities(new FakeLogger()), new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        public void LoadShouldParseAFileWithExtraColumns()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = f => AnzChequeCsvTestData.TestData2();
            StatementModel result = subject.Load("foo.bar", StatementModelTestData.ChequeAccount);

            Assert.AreEqual(1, result.DurationInMonths);
            Assert.AreEqual(7, result.AllTransactions.Count());
        }

        [TestMethod]
        public void LoadShouldParseAGoodFile()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = f => AnzChequeCsvTestData.TestData1();
            StatementModel result = subject.Load("foo.bar", StatementModelTestData.ChequeAccount);

            Assert.AreEqual(1, result.DurationInMonths);
            Assert.AreEqual(7, result.AllTransactions.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void LoadShouldThrowGivenBadData()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = filename => AnzChequeCsvTestData.BadTestData1();
            subject.Load("foo.bar", StatementModelTestData.ChequeAccount);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadShouldThrowIfFileNotFound()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            BankImportUtilities.AbortIfFileDoesntExistOverride = (s, m) => { throw new FileNotFoundException(); };
            subject.Load("foo.bar", StatementModelTestData.ChequeAccount);
            Assert.Fail();
        }

        [TestMethod]
        public void TatseTestShouldReturnFalseGivenABadFile()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadTextChunkOverride = file => "lkjpoisjg809wutwuoipsahf98qyfg0w9ashgpiosxnhbvoiyxcu8o9ui9paso,spotiw93th98sh8,35345345,353453534521,lkhsldhlsk,shgjkshj,sgsjdgsd";
            bool result = subject.TasteTest(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TatseTestShouldReturnFalseGivenAnEmptyTasteTestResponse()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadTextChunkOverride = file => string.Empty;
            bool result = subject.TasteTest(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TatseTestShouldReturnFalseGivenNullTasteTestResponse()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadTextChunkOverride = file => null;
            bool result = subject.TasteTest(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TatseTestShouldReturnFalseGivenTheChequeFormat()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            subject.ReadTextChunkOverride = file => "Atm Debit,Anz  1234567 Queen St,Anz  S3A1234,Queen St,Anch  123456,-80.00,16/06/2014,"; // Cheque format given to Visa parser
            bool result = subject.TasteTest(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TatseTestShouldReturnTrueGivenAGoodFile()
        {
            AnzVisaStatementImporterV1TestHarness subject = Arrange();
            bool result = subject.TasteTest(@"transumm.CSV");
            Assert.IsTrue(result);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            BankImportUtilities = new BankImportUtilitiesTestHarness(new FakeLogger());
        }

        private AnzVisaStatementImporterV1TestHarness Arrange()
        {
            return new AnzVisaStatementImporterV1TestHarness(new FakeUserMessageBox(), BankImportUtilities, new FakeLogger());
        }
    }
}