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
    public class AnzAccountStatementImporterV1Test
    {
        private BankImportUtilitiesTestHarness BankImportUtilities { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullBankImportUtilities()
        {
            new AnzAccountStatementImporterV1(new FakeUserMessageBox(), null, new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullLogger()
        {
            new AnzAccountStatementImporterV1(new FakeUserMessageBox(), new BankImportUtilities(new FakeLogger()), null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullMessageBox()
        {
            new AnzAccountStatementImporterV1(null, new BankImportUtilities(new FakeLogger()), new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        public void LoadShouldParseAGoodFile()
        {
            AnzAccountStatementImporterV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = f => AnzChequeCsvTestData.TestData1();
            StatementModel result = subject.Load("foo.bar", StatementModelTestData.ChequeAccount);

            Assert.AreEqual(1, result.DurationInMonths);
            Assert.AreEqual(7, result.AllTransactions.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadShouldThrowIfFileNotFound()
        {
            AnzAccountStatementImporterV1TestHarness subject = Arrange();
            BankImportUtilities.AbortIfFileDoesntExistOverride = (s, m) => { throw new FileNotFoundException(); };
            subject.Load("foo.bar", StatementModelTestData.ChequeAccount);
            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            BankImportUtilities = new BankImportUtilitiesTestHarness(new FakeLogger());
        }

        private AnzAccountStatementImporterV1TestHarness Arrange()
        {
            return new AnzAccountStatementImporterV1TestHarness(new FakeUserMessageBox(), BankImportUtilities, new FakeLogger());
        }
    }
}