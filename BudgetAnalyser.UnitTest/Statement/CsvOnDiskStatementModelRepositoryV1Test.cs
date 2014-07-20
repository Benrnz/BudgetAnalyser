using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xaml;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
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
        public void CtorShouldThrowGivenNullDomainMapper()
        {
            new CsvOnDiskStatementModelRepositoryV1(new FakeUserMessageBox(), new BankImportUtilities(new FakeLogger()), new FakeLogger(), new BasicMapper<TransactionSetDto, StatementModel>(), null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullBankImportUtils()
        {
            new CsvOnDiskStatementModelRepositoryV1(new FakeUserMessageBox(), null, new FakeLogger(), new BasicMapper<TransactionSetDto, StatementModel>(), new Mock<IStatementModelToTransactionSetDtoMapper>().Object);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullDtoMapper()
        {
            new CsvOnDiskStatementModelRepositoryV1(new FakeUserMessageBox(), new BankImportUtilities(new FakeLogger()), new FakeLogger(), null, new Mock<IStatementModelToTransactionSetDtoMapper>().Object);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullLogger()
        {
            new CsvOnDiskStatementModelRepositoryV1(new FakeUserMessageBox(), new BankImportUtilities(new FakeLogger()), null, new BasicMapper<TransactionSetDto, StatementModel>(), new Mock<IStatementModelToTransactionSetDtoMapper>().Object);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullMessageBox()
        {
            new CsvOnDiskStatementModelRepositoryV1(null, new BankImportUtilities(new FakeLogger()), new FakeLogger(), new BasicMapper<TransactionSetDto, StatementModel>(), new Mock<IStatementModelToTransactionSetDtoMapper>().Object);
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
            Console.WriteLine(result.LastImport);
            Assert.AreEqual(new DateTime(2012, 08, 20), result.LastImport);
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

        [TestMethod]
        public void MustBeAbleToLoadDemoStatementFile()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file =>
            {
                // this line of code is useful to figure out the name Vs has given the resource! The name is case sensitive.
                Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().ForEach(n => Debug.WriteLine(n));
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file))
                {
                    if (stream == null)
                    {
                        throw new MissingManifestResourceException("Cannot find resource named: " + file);
                    }

                    using (var streamReader = new StreamReader(stream))
                    {
                        var text = streamReader.ReadToEnd();
                        return text.Split('\n');
                    }
                }
            };

            var model = subject.Load(StatementDemoFile);

            Assert.IsNotNull(model);
            Assert.AreEqual(33, model.AllTransactions.Count());
        }

        private const string StatementDemoFile = "BudgetAnalyser.UnitTest.TestData.DemoTransactions.csv";

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