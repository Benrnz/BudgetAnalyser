using System;
using System.Data;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Statement
{
    [TestClass]
    public class CsvOnDiskStatementModelRepositoryV1Test
    {
        private const string StatementDemoFile = "BudgetAnalyser.UnitTest.TestData.DemoTransactions.csv";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullBankImportUtils()
        {
            new CsvOnDiskStatementModelRepositoryV1(new FakeUserMessageBox(), null, new FakeLogger(), new BasicMapperFake<TransactionSetDto, StatementModel>(),
                new BasicMapperFake<StatementModel, TransactionSetDto>());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullDomainMapper()
        {
            new CsvOnDiskStatementModelRepositoryV1(new FakeUserMessageBox(), new BankImportUtilities(new FakeLogger()), new FakeLogger(), new BasicMapperFake<TransactionSetDto, StatementModel>(),
                null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullDtoMapper()
        {
            new CsvOnDiskStatementModelRepositoryV1(new FakeUserMessageBox(), new BankImportUtilities(new FakeLogger()), new FakeLogger(), null,
                new BasicMapperFake<StatementModel, TransactionSetDto>());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullLogger()
        {
            new CsvOnDiskStatementModelRepositoryV1(new FakeUserMessageBox(), new BankImportUtilities(new FakeLogger()), null, new BasicMapperFake<TransactionSetDto, StatementModel>(),
                new BasicMapperFake<StatementModel, TransactionSetDto>());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullMessageBox()
        {
            new CsvOnDiskStatementModelRepositoryV1(null, new BankImportUtilities(new FakeLogger()), new FakeLogger(), new BasicMapperFake<TransactionSetDto, StatementModel>(),
                new BasicMapperFake<StatementModel, TransactionSetDto>());
            Assert.Fail();
        }

        [TestMethod]
        public void IsValidFileShouldReturnFalseGivenIncorrectVersionHashFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectVersionHash();
            bool result = subject.IsValidFileAsync("Foo.foo");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidFileShouldReturnTrueGivenGoodFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            bool result = subject.IsValidFileAsync("Foo.foo");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LoadShouldReturnAStatementModelGivenFileWithNoTransactions()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            StatementModel result = subject.LoadAsync("Foo.foo");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithFilenameGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            StatementModel result = subject.LoadAsync("Foo.foo");

            Assert.AreEqual("Foo.foo", result.FileName);
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithImportedDateGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            StatementModel result = subject.LoadAsync("Foo.foo");
            Console.WriteLine(result.LastImport);
            Assert.AreEqual(new DateTime(2012, 08, 20), result.LastImport);
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithNoTransactionsGivenFileWithNoTransactions()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            StatementModel result = subject.LoadAsync("Foo.foo");

            Assert.AreEqual(0, result.AllTransactions.Count());
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithOneDurationGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            StatementModel result = subject.LoadAsync("Foo.foo");

            Assert.AreEqual(1, result.DurationInMonths);
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithTransactionsGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            StatementModel result = subject.LoadAsync("Foo.foo");

            Assert.AreEqual(15, result.AllTransactions.Count());
        }

        [TestMethod]
        public void LoadShouldReturnStatementModelWithZeroDurationGivenFileWithNoTransactions()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            StatementModel result = subject.LoadAsync("Foo.foo");

            Assert.AreEqual(0, result.DurationInMonths);
        }

        [TestMethod]
        [ExpectedException(typeof(StatementModelChecksumException))]
        public void LoadShouldThrowGivenFileWithIncorrectChecksum()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectChecksum();
            subject.LoadAsync("foo.foo");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowGivenFileWithIncorrectDataTypes()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectDataTypeInRow1();
            subject.LoadAsync("foo.foo");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(VersionNotFoundException))]
        public void LoadShouldThrowGivenIncorrectVersionHashFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectVersionHash();
            subject.LoadAsync("Foo.foo");

            Assert.Fail();
        }

        [TestMethod]
        public void MustBeAbleToLoadDemoStatementFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = EmbeddedResourceHelper.ExtractString;

            StatementModel model = subject.LoadAsync(StatementDemoFile);

            Assert.IsNotNull(model);
            Assert.AreEqual(33, model.AllTransactions.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(StatementModelChecksumException))]
        public void SaveShouldThrowGivenMappingDoesNotMapAllTransactions()
        {
            var mockToDtoMapper = new Mock<BasicMapper<StatementModel, TransactionSetDto>>();
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = ArrangeWithMockMappers(new BasicMapperFake<TransactionSetDto, StatementModel>(), mockToDtoMapper.Object);
            StatementModel model = StatementModelTestData.TestData2();
            model.Filter(new GlobalFilterCriteria { BeginDate = new DateTime(2013, 07, 20), EndDate = new DateTime(2013, 08, 19) });

            mockToDtoMapper.Setup(m => m.Map(model)).Returns(
                new TransactionSetDto
                {
                    FileName = "Foo.bar",
                    LastImport = new DateTime(2013, 07, 20),
                    Transactions = TransactionSetDtoTestData.TestData2().Transactions.Take(2).ToList(),
                });

            subject.Save(model, "Foo.bar");

            Assert.Fail();
        }

        private CsvOnDiskStatementModelRepositoryV1TestHarness Arrange()
        {
            return new CsvOnDiskStatementModelRepositoryV1TestHarness(new FakeUserMessageBox(), new BankImportUtilitiesTestHarness());
        }

        private CsvOnDiskStatementModelRepositoryV1TestHarness ArrangeWithMockMappers(BasicMapper<TransactionSetDto, StatementModel> toDomainMapper,
            BasicMapper<StatementModel, TransactionSetDto> toDtoMapper)
        {
            return new CsvOnDiskStatementModelRepositoryV1TestHarness(new FakeUserMessageBox(), new BankImportUtilitiesTestHarness(), toDomainMapper, toDtoMapper);
        }
    }
}