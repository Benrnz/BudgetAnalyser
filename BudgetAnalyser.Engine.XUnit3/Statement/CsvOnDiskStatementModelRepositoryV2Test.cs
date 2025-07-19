using System.Text;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Statement;

public class CsvOnDiskStatementModelRepositoryV2Test
{
    private readonly IFileReaderWriter mockFileReaderWriter;
    private readonly IReaderWriterSelector mockReaderWriterSelector;
    private readonly ITestOutputHelper writer;

    public CsvOnDiskStatementModelRepositoryV2Test(ITestOutputHelper writer)
    {
        this.writer = writer;
        this.mockFileReaderWriter = Substitute.For<IFileReaderWriter>();
        this.mockReaderWriterSelector = Substitute.For<IReaderWriterSelector>();
        this.mockReaderWriterSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockFileReaderWriter);
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullBankImportUtils()
    {
        Should.Throw<ArgumentNullException>(() =>
            new CsvOnDiskStatementModelRepositoryV2(
                null!,
                new FakeLogger(),
                new DtoMapperStub<TransactionSetDto, TransactionSetModel>(),
                this.mockReaderWriterSelector));
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullLogger()
    {
        Should.Throw<ArgumentNullException>(() =>
            new CsvOnDiskStatementModelRepositoryV2(
                new BankImportUtilities(new FakeLogger()),
                null!,
                new DtoMapperStub<TransactionSetDto, TransactionSetModel>(),
                this.mockReaderWriterSelector));
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullMapper()
    {
        Should.Throw<ArgumentNullException>(() =>
            new CsvOnDiskStatementModelRepositoryV2(
                new BankImportUtilities(new FakeLogger()),
                new FakeLogger(),
                null!,
                this.mockReaderWriterSelector));
    }

    [Fact]
    public async Task Load_ShouldReturnAStatementModel_GivenFileWithNoTransactions()
    {
        var subject = ArrangeWithMocks();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);

        var model = await subject.LoadAsync("Foo.foo", false);

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithFilename_GivenTestData1()
    {
        var subject = ArrangeWithMocks();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.StorageKey.ShouldBe("Foo.foo");
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithImportedDate_GivenTestData1()
    {
        var subject = ArrangeWithMocks();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.LastImport.ShouldBe(new DateTime(new DateOnly(2012, 08, 20), TimeOnly.MinValue, DateTimeKind.Utc).ToLocalTime());
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithNoTransactions_GivenFileWithNoTransactions()
    {
        var subject = ArrangeWithMocks();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.AllTransactions.Count().ShouldBe(0);
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithOneDuration_GivenTestData1()
    {
        var subject = ArrangeWithMocks();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.TestData1()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.DurationInMonths.ShouldBe(1);
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithTransactions_GivenDemoFile()
    {
        var subject = ArrangeWithEmbeddedResources();
        var model = await subject.LoadAsync(TestDataConstants.DemoTransactionsFileName, false);

        model.AllTransactions.Count().ShouldBe(33);
        model.DurationInMonths.ShouldBe(1);
        model.LastImport.ShouldBe(DateTime.Parse("2014-07-19T21:15:20.0069564+12:00"));
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithTransactions_GivenTestData1()
    {
        var subject = ArrangeWithMocks();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.TestData1()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.AllTransactions.Count().ShouldBe(15);
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithZeroDuration_GivenFileWithNoTransactions()
    {
        var subject = ArrangeWithMocks();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.DurationInMonths.ShouldBe(0);
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenFileWithIncorrectChecksum()
    {
        var subject = ArrangeWithMocks();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectChecksum()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);

        await Should.ThrowAsync<TransactionsModelChecksumException>(() => subject.LoadAsync("foo.foo", false));
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenFileWithIncorrectDataTypes()
    {
        var subject = ArrangeWithMocks();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectDataTypeInRow1()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);

        await Should.ThrowAsync<DataFormatException>(() => subject.LoadAsync("foo.foo", false));
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenIncorrectVersionHashFile()
    {
        var subject = ArrangeWithMocks();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectVersionHash()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);

        await Should.ThrowAsync<DataFormatException>(() => subject.LoadAsync("Foo.foo", false));
    }

    [Fact]
    public async Task LoadAndSave_ShouldProduceTheSameCsv_GivenDemoFile()
    {
        var subject = ArrangeWithEmbeddedResources();
        var model = await subject.LoadAsync(TestDataConstants.DemoTransactionsFileName, false);

        await subject.SaveAsync(model, false);
        var result = subject.SerialisedData.Trim();
        var expected = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.DemoTransactionsFileName).Trim();

        result.ShouldBe(expected);
    }

    [Fact]
    public async Task SaveAsync_ShouldWriteSerializedDataToFile_GivenTestData2()
    {
        // Arrange
        var subject = ArrangeWithMocks();
        var testData = StatementModelTestData.TestData2();
        subject.WriteStream = new MemoryStream();

        // Act
        await subject.SaveAsync(testData, false);

        // Assert
        subject.SerialisedData.ShouldNotBeNullOrEmpty();
        subject.SerialisedData.ShouldContain("VersionHash,15955E20-A2CC-4C69-AD42-94D84377FC0C,TransactionCheckSum,-8509267440001667191,2013-08-14T12:00:00.0000000Z");
    }

    [Fact]
    public async Task WrittenDataShouldAutomaticallyStripCommas()
    {
        var subject = ArrangeWithMocks();
        subject.Dto = BudgetAnalyserRawCsvTestDataV1.BadTestData_CorruptedCommaFormat();
        var writerStream = new MemoryStream();
        this.mockFileReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(writerStream);
        await subject.SaveAsync(StatementModelTestData.TestData1(), false);

        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(subject.SerialisedData));
        var reader = new StreamReader(memoryStream);
        var firstLine = await reader.ReadLineAsync(TestContext.Current.CancellationToken);
        var secondLine = await reader.ReadLineAsync(TestContext.Current.CancellationToken);
        firstLine!.Count(c => c == ',').ShouldBe(4);
        secondLine!.Count(c => c == ',').ShouldBe(10);
    }

    private CsvOnDiskStatementModelRepositoryV2TestHarness ArrangeWithEmbeddedResources()
    {
        var logger = new XUnitLogger(this.writer);
        var realMapper = new MapperStatementModelToDto2(new InMemoryAccountTypeRepository(), new BudgetBucketRepoAlwaysFind(), new InMemoryTransactionTypeRepository(), logger);
        var selector = new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), new EmbeddedResourceFileReaderWriterEncrypted()]);
        return new CsvOnDiskStatementModelRepositoryV2TestHarness(new XUnitLogger(this.writer), realMapper, selector);
    }

    private CsvOnDiskStatementModelRepositoryV2TestHarness ArrangeWithMocks()
    {
        var logger = new XUnitLogger(this.writer);
        var realMapper = new MapperStatementModelToDto2(new InMemoryAccountTypeRepository(), new BudgetBucketRepoAlwaysFind(), new InMemoryTransactionTypeRepository(), logger);
        return new CsvOnDiskStatementModelRepositoryV2TestHarness(new XUnitLogger(this.writer), realMapper, this.mockReaderWriterSelector);
    }
}
