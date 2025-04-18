﻿using System.Text;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.UnitTest.Statement;

public class CsvOnDiskStatementModelRepositoryV1Test
{
    private readonly IDtoMapper<TransactionSetDto, StatementModel> mapper;
    private readonly IFileReaderWriter mockFileReaderWriter;
    private readonly IReaderWriterSelector mockReaderWriterSelector;
    private readonly ITestOutputHelper writer;

    public CsvOnDiskStatementModelRepositoryV1Test(ITestOutputHelper writer)
    {
        this.writer = writer;
        this.mapper = Substitute.For<IDtoMapper<TransactionSetDto, StatementModel>>();
        this.mockFileReaderWriter = Substitute.For<IFileReaderWriter>();
        this.mockReaderWriterSelector = Substitute.For<IReaderWriterSelector>();
        this.mockReaderWriterSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockFileReaderWriter);
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullBankImportUtils()
    {
        Should.Throw<ArgumentNullException>(() =>
            new CsvOnDiskStatementModelRepositoryV1(
                null,
                new FakeLogger(),
                new DtoMapperStub<TransactionSetDto, StatementModel>(),
                this.mockReaderWriterSelector));
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullLogger()
    {
        Should.Throw<ArgumentNullException>(() =>
            new CsvOnDiskStatementModelRepositoryV1(
                new BankImportUtilities(new FakeLogger()),
                null,
                new DtoMapperStub<TransactionSetDto, StatementModel>(),
                this.mockReaderWriterSelector));
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullMapper()
    {
        Should.Throw<ArgumentNullException>(() =>
            new CsvOnDiskStatementModelRepositoryV1(
                new BankImportUtilities(new FakeLogger()),
                new FakeLogger(),
                null,
                this.mockReaderWriterSelector));
    }

    [Fact]
    public async Task Load_ShouldReturnAStatementModel_GivenFileWithNoTransactions()
    {
        var subject = Arrange();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);

        var model = await subject.LoadAsync("Foo.foo", false);

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithFilename_GivenTestData1()
    {
        var subject = Arrange();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.StorageKey.ShouldBe("Foo.foo");
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithImportedDate_GivenTestData1()
    {
        var subject = Arrange();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.LastImport.ShouldBe(new DateTime(new DateOnly(2012, 08, 20), TimeOnly.MinValue, DateTimeKind.Utc).ToLocalTime());
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithNoTransactions_GivenFileWithNoTransactions()
    {
        var subject = Arrange();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.AllTransactions.Count().ShouldBe(0);
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithOneDuration_GivenTestData1()
    {
        var subject = Arrange();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.DurationInMonths.ShouldBe(1);
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithTransactions_GivenTestData1()
    {
        var subject = Arrange();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.AllTransactions.Count().ShouldBe(15);
    }

    [Fact]
    public async Task Load_ShouldReturnStatementModelWithZeroDuration_GivenFileWithNoTransactions()
    {
        var subject = Arrange();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.EmptyTestData()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);
        var model = await subject.LoadAsync("Foo.foo", false);

        model.DurationInMonths.ShouldBe(0);
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenFileWithIncorrectChecksum()
    {
        var subject = Arrange();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectChecksum()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);

        await Should.ThrowAsync<StatementModelChecksumException>(() => subject.LoadAsync("foo.foo", false));
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenFileWithIncorrectDataTypes()
    {
        var subject = Arrange();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectDataTypeInRow1()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);

        await Should.ThrowAsync<DataFormatException>(() => subject.LoadAsync("foo.foo", false));
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenIncorrectVersionHashFile()
    {
        var subject = Arrange();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(BudgetAnalyserRawCsvTestDataV1.BadTestData_IncorrectVersionHash()));
        this.mockFileReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(stream);

        await Should.ThrowAsync<DataFormatException>(() => subject.LoadAsync("Foo.foo", false));
    }

    [Fact]
    public async Task WrittenDataShouldAutomaticallyStripCommas()
    {
        var subject = Arrange();
        subject.Dto = BudgetAnalyserRawCsvTestDataV1.BadTestData_CorruptedCommaFormat();
        await subject.SaveAsync(StatementModelTestData.TestData1(), "Foo.bar", false);

        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(subject.SerialisedData));
        var reader = new StreamReader(memoryStream);
        var firstLine = await reader.ReadLineAsync(TestContext.Current.CancellationToken);
        var secondLine = await reader.ReadLineAsync(TestContext.Current.CancellationToken);
        firstLine!.Count(c => c == ',').ShouldBe(4);
        secondLine!.Count(c => c == ',').ShouldBe(10);
    }

    private CsvOnDiskStatementModelRepositoryV2TestHarness Arrange()
    {
        return new CsvOnDiskStatementModelRepositoryV2TestHarness(new XUnitLogger(this.writer), this.mapper, this.mockReaderWriterSelector);
    }
}
