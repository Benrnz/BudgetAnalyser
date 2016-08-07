using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.TangyFruitMapper;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Statement
{
    [TestClass]
    public class CsvOnDiskStatementModelRepositoryV1Test
    {
        private Mock<IReaderWriterSelector> mockReaderWriterSelector;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ShouldThrow_GivenNullBankImportUtils()
        {
            new CsvOnDiskStatementModelRepositoryV1(
                null,
                new FakeLogger(),
                new DtoMapperStub<TransactionSetDto, StatementModel>(),
                this.mockReaderWriterSelector.Object);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ShouldThrow_GivenNullLogger()
        {
            new CsvOnDiskStatementModelRepositoryV1(
                new BankImportUtilities(new FakeLogger()),
                null,
                new DtoMapperStub<TransactionSetDto, StatementModel>(),
                this.mockReaderWriterSelector.Object);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ShouldThrow_GivenNullMapper()
        {
            new CsvOnDiskStatementModelRepositoryV1(
                new BankImportUtilities(new FakeLogger()),
                new FakeLogger(),
                null,
                this.mockReaderWriterSelector.Object);
            Assert.Fail();
        }

        [TestMethod]
        public async Task IsValidFile_ShouldReturnFalse_GivenIncorrectVersionHashFile()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectVersionHash();
            var result = await subject.IsStatementModelAsync("Foo.foo", false);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsValidFile_ShouldReturnTrue_GivenGoodFile()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            var result = await subject.IsStatementModelAsync("Foo.foo", false);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Load_ShouldReturnAStatementModel_GivenFileWithNoTransactions()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            var model = await subject.LoadAsync("Foo.foo", false);

            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task Load_ShouldReturnStatementModelWithFilename_GivenTestData1()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            var model = await subject.LoadAsync("Foo.foo", false);

            Assert.AreEqual("Foo.foo", model.StorageKey);
        }

        [TestMethod]
        public async Task Load_ShouldReturnStatementModelWithImportedDate_GivenTestData1()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            var model = await subject.LoadAsync("Foo.foo", false);
            Console.WriteLine(model.LastImport);
            Assert.AreEqual(new DateTime(2012, 08, 20), model.LastImport);
        }

        [TestMethod]
        public async Task Load_ShouldReturnStatementModelWithNoTransactions_GivenFileWithNoTransactions()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            var model = await subject.LoadAsync("Foo.foo", false);

            Assert.AreEqual(0, model.AllTransactions.Count());
        }

        [TestMethod]
        public async Task Load_ShouldReturnStatementModelWithOneDuration_GivenTestData1()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            var model = await subject.LoadAsync("Foo.foo", false);

            Assert.AreEqual(1, model.DurationInMonths);
        }

        [TestMethod]
        public async Task Load_ShouldReturnStatementModelWithTransactions_GivenTestData1()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.TestData1();
            var model = await subject.LoadAsync("Foo.foo", false);

            Assert.AreEqual(15, model.AllTransactions.Count());
        }

        [TestMethod]
        public async Task Load_ShouldReturnStatementModelWithZeroDuration_GivenFileWithNoTransactions()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.EmptyTestData();
            var model = await subject.LoadAsync("Foo.foo", false);

            Assert.AreEqual(0, model.DurationInMonths);
        }

        [TestMethod]
        [ExpectedException(typeof(StatementModelChecksumException))]
        public async Task Load_ShouldThrow_GivenFileWithIncorrectChecksum()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectChecksum();
            await subject.LoadAsync("foo.foo", false);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task Load_ShouldThrow_GivenFileWithIncorrectDataTypes()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectDataTypeInRow1();
            await subject.LoadAsync("foo.foo", false);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task Load_ShouldThrow_GivenIncorrectVersionHashFile()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectVersionHash();
            await subject.LoadAsync("Foo.foo", false);

            Assert.Fail();
        }

        [TestMethod]
        public async Task MustBeAbleToLoadDemoStatementFile()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = x => GetType().Assembly.ExtractEmbeddedResourceAsLines(x);

            var model = await subject.LoadAsync(TestDataConstants.DemoTransactionsFileName, false);

            Assert.IsNotNull(model);
            Assert.AreEqual(33, model.AllTransactions.Count());
        }

        [TestMethod]
        public async Task MustBeAbleToLoadDemoStatementFile2()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = file => GetType().Assembly.ExtractEmbeddedResourceAsLines(TestDataConstants.DemoTransactionsFileName, true);
            var model = await subject.LoadAsync("Foo.foo", false);
            Console.WriteLine(model.DurationInMonths);
            Assert.AreEqual(1, model.DurationInMonths);
        }

        [TestMethod]
        [ExpectedException(typeof(StatementModelChecksumException))]
        public async Task Save_ShouldThrow_GivenMappingDoesNotMapAllTransactions()
        {
            var mapper = new Mock<IDtoMapper<TransactionSetDto, StatementModel>>();
            var subject = ArrangeWithMockMappers(mapper.Object);
            var model = StatementModelTestData.TestData2();
            model.Filter(new GlobalFilterCriteria { BeginDate = new DateTime(2013, 07, 20), EndDate = new DateTime(2013, 08, 19) });

            mapper.Setup(m => m.ToDto(model)).Returns(
                new TransactionSetDto
                {
                    StorageKey = "Foo.bar",
                    LastImport = new DateTime(2013, 07, 20),
                    Transactions = TransactionSetDtoTestData.TestData2().Transactions.Take(2).ToList()
                });

            await subject.SaveAsync(model, "Foo.bar", false);

            Assert.Fail();
        }

        [TestInitialize]
        public void TestSetup()
        {
            this.mockReaderWriterSelector = new Mock<IReaderWriterSelector>();
        }

        private CsvOnDiskStatementModelRepositoryV1TestHarness Arrange()
        {
            return new CsvOnDiskStatementModelRepositoryV1TestHarness(new BankImportUtilitiesTestHarness(), this.mockReaderWriterSelector.Object);
        }

        private CsvOnDiskStatementModelRepositoryV1TestHarness ArrangeWithMockMappers(IDtoMapper<TransactionSetDto, StatementModel> mapper)
        {
            return new CsvOnDiskStatementModelRepositoryV1TestHarness(new BankImportUtilitiesTestHarness(), mapper, this.mockReaderWriterSelector.Object);
        }
    }
}