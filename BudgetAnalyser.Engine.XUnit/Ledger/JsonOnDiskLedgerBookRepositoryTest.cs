using System.Diagnostics;
using System.Text;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class JsonOnDiskLedgerBookRepositoryTest : IDisposable
{
    private const string LoadFileName = @"BudgetAnalyser.Engine.XUnit.TestData.LedgerBookRepositoryTest_Load_ShouldLoadTheJsonFile.json";
    private readonly ILogger logger;

    private readonly IDtoMapper<LedgerBookDto, LedgerBook> mapper;
    private readonly IFileReaderWriter mockReaderWriter = Substitute.For<IFileReaderWriter>();
    private readonly IReaderWriterSelector mockReaderWriterSelector = Substitute.For<IReaderWriterSelector>();
    private readonly XUnitOutputWriter outputter;
    private readonly Stopwatch stopwatch;

    public JsonOnDiskLedgerBookRepositoryTest(ITestOutputHelper output)
    {
        this.outputter = new XUnitOutputWriter(output);
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        var accountRepo = new InMemoryAccountTypeRepository();
        this.mapper = new MapperLedgerBookToDto2(
            bucketRepo,
            accountRepo,
            new LedgerBucketFactory(bucketRepo, accountRepo),
            new LedgerTransactionFactory());

        this.logger = new XUnitLogger(output);

        this.stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        this.outputter?.WriteLine($"TOTAL TIME: {this.stopwatch.Elapsed}");
        this.outputter?.Dispose();
    }

    [Fact(Skip = "This test is for manual use only")]
    public async Task ConvertDemoFrom_DemoLedgerBookJson()
    {
        var xamlRepo = new XamlOnDiskLedgerBookRepositoryTestHarness(
            this.mapper,
            new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), new EmbeddedResourceFileReaderWriterEncrypted()]));
        var ledgerBook = await xamlRepo.LoadAsync(@"BudgetAnalyser.Engine.XUnit.TestData.LedgerBookRepositoryTest_Load_ShouldLoadTheJsonFile.json", false);

        var subject = CreateSubject();
        this.mockReaderWriterSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockReaderWriter);
        var myStream = new MemoryStream();
        this.mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);
        await subject.SaveAsync(ledgerBook, TestDataConstants.DemoLedgerBookFileName, false);

        this.outputter.WriteLine(subject.SerialisedData);
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task DemoBookFileChecksum_ShouldNotChange_WhenLoadAndSave()
    {
        var subject = CreateSubject(true);
        LedgerBookDto loadedDto = null;
        LedgerBookDto savedDto = null;

        var book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileNameJson, false);
        loadedDto = subject.Dto;
        loadedDto.Output(true, this.outputter);

        await subject.SaveAsync(book, book.StorageKey, false);
        savedDto = subject.Dto;
        savedDto.Output(true, this.outputter);

        savedDto.Checksum.ShouldBe(loadedDto.Checksum);

        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Load_Output()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);

        // Visual compare these two - should be the same
        LedgerBookTestData.TestData2().Output(outputWriter: this.outputter);

        book.Output(outputWriter: this.outputter);
    }

    [Fact]
    public async Task Load_ShouldCreateBookThatIsValid()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var builder = new StringBuilder();
        book.Validate(builder).ShouldBeTrue(builder.ToString());
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithFirstLineEqualBankBalances()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();
        var line = book.Reconciliations.First();

        line.TotalBankBalance.ShouldBe(testData2.Reconciliations.First().TotalBankBalance);
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithFirstLineEqualSurplus()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        book.Output(outputWriter: this.outputter);

        var testData2 = LedgerBookTestData.TestData2();
        testData2.Output(outputWriter: this.outputter);

        var line = book.Reconciliations.First();

        line.CalculatedSurplus.ShouldBe(testData2.Reconciliations.First().CalculatedSurplus);
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameModifiedDate()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Modified.ShouldBe(testData2.Modified);
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameName()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Name.ShouldBe(testData2.Name);
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameNumberOfLedgers()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Ledgers.Count().ShouldBe(testData2.Ledgers.Count());
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameNumberOfReconciliations()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Reconciliations.Count().ShouldBe(testData2.Reconciliations.Count());
    }

    [Fact]
    public async Task Load_ShouldLoadTheJsonFile()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);

        book.ShouldNotBeNull();
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task MustBeAbleToLoadDemoLedgerBookFile()
    {
        var subject = CreateSubject(true);

        var book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileNameJson, false);
        book.Output(true, this.outputter);
        book.ShouldNotBeNull();
    }

    [Fact]
    public async Task Save_ShouldSaveTheJsonFile()
    {
        var fileName = @"CompleteSmellyFoo.json";

        var subject = CreateSubject(true);

        var testData = LedgerBookTestData.TestData2();
        await subject.SaveAsync(testData, fileName, false);

        subject.SerialisedData.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SaveAsync_ShouldHaveACheckSumOf8435_GivenLedgerBookTestData2()
    {
        var subject = CreateSubject(true);

        await subject.SaveAsync(LedgerBookTestData.TestData2(), "Foo.json", false);

        ExtractCheckSum(subject.SerialisedData).ShouldBe(8435.06);
    }

    [Fact]
    public async Task SavingAndLoading_ShouldProduceTheSameCheckSum()
    {
        var subject1 = CreateSubject(true);
        await subject1.SaveAsync(LedgerBookTestData.TestData2(), "Foo2.json", false);
        var serialisedData = subject1.SerialisedData;
        this.outputter.WriteLine("Saved / Serialised Json:");
        this.outputter.WriteLine(serialisedData);

        var subject2 = CreateSubject();
        this.mockReaderWriterSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockReaderWriter);
        using var myStream = new MemoryStream(Encoding.UTF8.GetBytes(serialisedData));
        this.mockReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(myStream);
        await subject2.LoadAsync("foo", false);
        var bookDto = subject2.Dto;

        bookDto.Checksum.ShouldBe(ExtractCheckSum(serialisedData));
    }

    [Fact]
    public async Task SerialiseTestData2ToEnsureItMatches_Load_ShouldLoadTheJsonFile_json()
    {
        var subject = CreateSubject(true);

        await subject.SaveAsync(LedgerBookTestData.TestData2(), "Leonard Nimoy.json", false);
        var serialisedData = subject.SerialisedData;

        this.outputter.WriteLine(serialisedData);

        serialisedData.Length.ShouldBeGreaterThan(100);
    }

    private JsonOnDiskLedgerBookRepositoryTestHarness CreateSubject(bool real = false)
    {
        var importUtilities = new BankImportUtilitiesTestHarness(this.logger);
        if (real)
        {
            // Use real classes to operate very closely to live mode.
            return new JsonOnDiskLedgerBookRepositoryTestHarness(
                this.mapper,
                importUtilities,
                new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), new EmbeddedResourceFileReaderWriterEncrypted()]), this.logger);
        }

        // Use fake and mock objects where possible to better isolate testing.
        return new JsonOnDiskLedgerBookRepositoryTestHarness(
            this.mapper,
            importUtilities,
            this.mockReaderWriterSelector, this.logger);
    }

    private double ExtractCheckSum(string json)
    {
        var checksumPosition = json.IndexOf("\"CheckSum\": ", StringComparison.OrdinalIgnoreCase);
        var checksumLength = json.IndexOf(',', checksumPosition + 12) - checksumPosition;
        var serialisedCheckSum = json.Substring(checksumPosition + 12, checksumLength - 12);
        return double.Parse(serialisedCheckSum);
    }
}
