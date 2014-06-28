using System;
using System.Data;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Statement
{
    [TestClass]
    public class CsvOnDiskStatementModelRepositoryV1Test
    {
        private Mock<IAccountTypeRepository> AccountTypeRepoMock { get; set; }

        private Mock<IBudgetBucketRepository> BudgetBucketRepoMock { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullAccountTypeRepo()
        {
            new CsvOnDiskStatementModelRepositoryV1(null, new FakeUserMessageBox(), BudgetBucketRepoMock.Object, new BankImportUtilities(new FakeLogger()), new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullBankImportUtils()
        {
            new CsvOnDiskStatementModelRepositoryV1(AccountTypeRepoMock.Object, new FakeUserMessageBox(), BudgetBucketRepoMock.Object, null, new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullBudgetBucketRepo()
        {
            new CsvOnDiskStatementModelRepositoryV1(AccountTypeRepoMock.Object, new FakeUserMessageBox(), null, new BankImportUtilities(new FakeLogger()), new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullLogger()
        {
            new CsvOnDiskStatementModelRepositoryV1(AccountTypeRepoMock.Object, new FakeUserMessageBox(), BudgetBucketRepoMock.Object, new BankImportUtilities(new FakeLogger()), null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullMessageBox()
        {
            new CsvOnDiskStatementModelRepositoryV1(AccountTypeRepoMock.Object, null, BudgetBucketRepoMock.Object, new BankImportUtilities(new FakeLogger()), new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        public void IsValidFileShouldReturnFalseGivenIncorrectVersionHashFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectVersionHash();
            bool result = subject.IsValidFile("Foo.foo");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidFileShouldReturnTrueGivenGoodFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            bool result = subject.IsValidFile("Foo.foo");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LoadShouldReturnAStatementModelGivenFileWithNoTransactions()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            StatementModel result = subject.Load("Foo.foo");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithFilenameGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            StatementModel result = subject.Load("Foo.foo");

            Assert.AreEqual("Foo.foo", result.FileName);
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithImportedDateGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            StatementModel result = subject.Load("Foo.foo");
            Console.WriteLine(result.Imported);
            Assert.AreEqual(new DateTime(2012, 08, 20), result.Imported);
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithNoTransactionsGivenFileWithNoTransactions()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            StatementModel result = subject.Load("Foo.foo");

            Assert.AreEqual(0, result.AllTransactions.Count());
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithOneDurationGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            StatementModel result = subject.Load("Foo.foo");

            Assert.AreEqual(1, result.DurationInMonths);
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithTransactionsGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            StatementModel result = subject.Load("Foo.foo");

            Assert.AreEqual(15, result.AllTransactions.Count());
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithZeroDurationGivenFileWithNoTransactions()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            StatementModel result = subject.Load("Foo.foo");

            Assert.AreEqual(0, result.DurationInMonths);
        }

        [TestMethod]
        [ExpectedException(typeof(StatementModelChecksumException))]
        public void LoadShouldThrowGivenFileWithIncorrectChecksum()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectChecksum();
            subject.Load("foo.foo");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowGivenFileWithIncorrectDataTypes()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectDataTypeInRow1();
            subject.Load("foo.foo");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(VersionNotFoundException))]
        public void LoadShouldThrowGivenIncorrectVersionHashFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectVersionHash();
            subject.Load("Foo.foo");

            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            AccountTypeRepoMock = new Mock<IAccountTypeRepository>();
            BudgetBucketRepoMock = new Mock<IBudgetBucketRepository>();
        }

        private CsvOnDiskStatementModelRepositoryV1TestHarness Arrange()
        {
            return new CsvOnDiskStatementModelRepositoryV1TestHarness(
                AccountTypeRepoMock.Object,
                new FakeUserMessageBox(),
                BudgetBucketRepoMock.Object,
                new BankImportUtilitiesTestHarness(new FakeLogger()),
                new FakeLogger());
        }
    }
}