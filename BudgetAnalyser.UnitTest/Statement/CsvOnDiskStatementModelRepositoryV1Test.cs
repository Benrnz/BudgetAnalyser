using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task IsValidFileShouldReturnFalseGivenIncorrectVersionHashFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectVersionHash();
            var result = await subject.IsValidFileAsync("Foo.foo");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsValidFileShouldReturnTrueGivenGoodFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            var result = await subject.IsValidFileAsync("Foo.foo");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task LoadShouldReturnAStatementModelGivenFileWithNoTransactions()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            var model = await subject.LoadAsync("Foo.foo");

            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task LoadShouldReturnStatementModelWithFilenameGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            var model = await subject.LoadAsync("Foo.foo");

            Assert.AreEqual("Foo.foo", model.FileName);
        }

        [TestMethod]
        public async Task LoadShouldReturnStatementModelWithImportedDateGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            var model = await subject.LoadAsync("Foo.foo");
            Console.WriteLine(model.LastImport);
            Assert.AreEqual(new DateTime(2012, 08, 20), model.LastImport);
        }

        [TestMethod]
        public async Task LoadShouldReturnStatementModelWithNoTransactionsGivenFileWithNoTransactions()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            var model = await subject.LoadAsync("Foo.foo");

            Assert.AreEqual(0, model.AllTransactions.Count());
        }

        [TestMethod]
        public async Task LoadShouldReturnStatementModelWithOneDurationGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            var model = await subject.LoadAsync("Foo.foo");

            Assert.AreEqual(1, model.DurationInMonths);
        }

        [TestMethod]
        public async Task LoadShouldReturnStatementModelWithTransactionsGivenTestData1()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            var model = await subject.LoadAsync("Foo.foo");

            Assert.AreEqual(15, model.AllTransactions.Count());
        }

        [TestMethod]
        public async Task LoadShouldReturnStatementModelWithZeroDurationGivenFileWithNoTransactions()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            var model = await subject.LoadAsync("Foo.foo");

            Assert.AreEqual(0, model.DurationInMonths);
        }

        [TestMethod]
        [ExpectedException(typeof(StatementModelChecksumException))]
        public async Task LoadShouldThrowGivenFileWithIncorrectChecksum()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectChecksum();
            await subject.LoadAsync("foo.foo");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public async Task LoadShouldThrowGivenFileWithIncorrectDataTypes()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectDataTypeInRow1();
            await subject.LoadAsync("foo.foo");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(VersionNotFoundException))]
        public async Task LoadShouldThrowGivenIncorrectVersionHashFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectVersionHash();
            await subject.LoadAsync("Foo.foo");

            Assert.Fail();
        }

        [TestMethod]
        public async Task MustBeAbleToLoadDemoStatementFile()
        {
            CsvOnDiskStatementModelRepositoryV1TestHarness subject = Arrange();
            subject.ReadLinesOverride = EmbeddedResourceHelper.ExtractString;

            var model = await subject.LoadAsync(StatementDemoFile);

            Assert.IsNotNull(model);
            Assert.AreEqual(33, model.AllTransactions.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(StatementModelChecksumException))]
        public async Task SaveShouldThrowGivenMappingDoesNotMapAllTransactions()
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

            await subject.SaveAsync(model, "Foo.bar");

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