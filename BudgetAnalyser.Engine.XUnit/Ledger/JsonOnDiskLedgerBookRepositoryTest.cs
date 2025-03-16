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
using Rees.UnitTestUtilities;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class JsonOnDiskLedgerBookRepositoryTest : IDisposable
{
    private const string LoadFileName = @"BudgetAnalyser.Engine.XUnit.TestData.LedgerBookRepositoryTest_Load_ShouldLoadTheJsonFile.json";
    private readonly EmbeddedResourceFileReaderWriterEncrypted encryptedReaderWriter = new();
    private readonly ILogger logger;

    private readonly IDtoMapper<LedgerBookDto, LedgerBook> mapper;
    private readonly IFileReaderWriter mockReaderWriter = Substitute.For<IFileReaderWriter>();
    private readonly IReaderWriterSelector mockReaderWriterSelector = Substitute.For<IReaderWriterSelector>();
    private readonly XUnitOutputWriter outputter;
    private readonly Stopwatch stopwatch;

    public JsonOnDiskLedgerBookRepositoryTest(ITestOutputHelper output)
    {
        this.outputter = new XUnitOutputWriter(output);
        this.logger = new XUnitLogger(output);
        var bucketRepo = new BudgetBucketRepoAlwaysFind();
        var accountRepo = new InMemoryAccountTypeRepository();
        this.mapper = new MapperLedgerBookToDto2(
            bucketRepo,
            accountRepo,
            new LedgerBucketFactory(bucketRepo, accountRepo),
            new LedgerTransactionFactory(),
            this.logger);

        this.stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        this.outputter?.WriteLine($"TOTAL TIME: {this.stopwatch.Elapsed}");
        this.outputter?.Dispose();
        this.encryptedReaderWriter.InputStream?.Dispose();
        this.encryptedReaderWriter.OutputStream?.Dispose();
    }

    [Fact]
    public async Task Load_ShouldCreateBookThatValidates_GivenJsonTestData()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var builder = new StringBuilder();
        book.Validate(builder).ShouldBeTrue(builder.ToString());
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithFirstLineEqualBankBalances_GivenTestData2()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();
        var line = book.Reconciliations.First();

        line.TotalBankBalance.ShouldBe(testData2.Reconciliations.First().TotalBankBalance);
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithFirstLineEqualSurplus_GivenTestData2()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        book.Output(outputWriter: this.outputter);

        var testData2 = LedgerBookTestData.TestData2();
        testData2.Output(outputWriter: this.outputter);

        var line = book.Reconciliations.First();

        line.CalculatedSurplus.ShouldBe(testData2.Reconciliations.First().CalculatedSurplus);
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameModifiedDate_GivenTestData2()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Modified.ShouldBe(testData2.Modified);
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameName_GivenTestData2()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Name.ShouldBe(testData2.Name);
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameNumberOfLedgers_GivenTestData2()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Ledgers.Count().ShouldBe(testData2.Ledgers.Count());
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameNumberOfReconciliations_GivenTestData2()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Reconciliations.Count().ShouldBe(testData2.Reconciliations.Count());
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Load_ShouldLoadDemoLedgerBookFile_GivenDemoBook()
    {
        var subject = CreateSubject(true);

        var book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName, false);
        book.Output(true, this.outputter);
        book.ShouldNotBeNull();
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Load_ShouldLoadTheJsonFile_GivenJsonTestData()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);

        book.ShouldNotBeNull();
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task LoadAndOutput_ShouldOutputRehydratedLedgerBook_GivenJsonTestData()
    {
        var subject = CreateSubject(true);
        var book = await subject.LoadAsync(LoadFileName, false);

        // Visual compare these two - should be the same
        LedgerBookTestData.TestData2().Output(outputWriter: this.outputter);

        book.Output(outputWriter: this.outputter);
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task LoadAndSave_ShouldNotChangeChecksum_GivenDemoBook()
    {
        var subject = CreateSubject(true);
        LedgerBookDto loadedDto = null;
        LedgerBookDto savedDto = null;

        var book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName, false);
        loadedDto = subject.Dto;
        loadedDto.Output(true, this.outputter);

        await subject.SaveAsync(book, book.StorageKey, false);
        savedDto = subject.Dto;
        subject.Dto = null;
        savedDto.Output(true, this.outputter);

        savedDto.Checksum.ShouldBe(loadedDto.Checksum);

        this.stopwatch.Stop();
    }

    [Fact]
    public async Task LoadEncrypted_ShouldHaveCheckSumOf2728_88()
    {
        var subject = CreateSubject(true);
        this.encryptedReaderWriter.InputStream = GetType().Assembly.GetManifestResourceStream(TestDataConstants.DemoLedgerBookFileName + ".secure");

        var book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName + ".secure", true);
        subject.Dto.Checksum.ShouldBe(2728.88);
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task LoadEncrypted_ShouldLoadDemoLedgerBookFile_GivenDemoBook()
    {
        var subject = CreateSubject(true);
        this.encryptedReaderWriter.InputStream = GetType().Assembly.GetManifestResourceStream(TestDataConstants.DemoLedgerBookFileName + ".secure");

        var book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName + ".secure", true);
        book.Output(true, this.outputter);
        book.ShouldNotBeNull();
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task LoadEncryptedAndSaveEncrypted_ShouldChecksum2728_GivenDemoLedgerBook()
    {
        var subject = CreateSubject(true);
        LedgerBookDto loadedDto = null;
        LedgerBookDto savedDto = null;

        this.encryptedReaderWriter.InputStream = GetType().Assembly.GetManifestResourceStream(TestDataConstants.DemoLedgerBookFileName + ".secure");
        var book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName + ".secure", true);
        loadedDto = subject.Dto;
        subject.Dto = null;

        await subject.SaveAsync(book, book.StorageKey, true);
        savedDto = subject.Dto;

        savedDto.Checksum.ShouldBe(loadedDto.Checksum);

        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Save_ShouldProduceKnownJson_GivenTestData2()
    {
        // Save TestData2 to produce the serialised data.
        var subject = CreateSubject(true);
        await subject.SaveAsync(LedgerBookTestData.TestData2(), LoadFileName, false);
        var serialisedData = subject.SerialisedData;
        this.outputter.WriteLine(serialisedData);
        serialisedData.Length.ShouldBeGreaterThan(100);

        // Load the serialised data from a known good data file for comparison.
        this.outputter.WriteLine("===================================== EXPECTED TEXT =====================================");
        var expectedText = GetType().Assembly.ExtractEmbeddedResourceAsText(LoadFileName).Trim();
        this.outputter.WriteLine(expectedText);

        expectedText = JsonHelper.MinifyJson(expectedText);
        serialisedData = JsonHelper.MinifyJson(serialisedData);

        serialisedData.ShouldBe(expectedText);
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Save_ShouldSaveTheJsonFile_GivenTestData2()
    {
        var fileName = @"CompleteSmellyFoo.json";

        var subject = CreateSubject(true);

        var testData = LedgerBookTestData.TestData2();
        await subject.SaveAsync(testData, fileName, false);

        subject.SerialisedData.ShouldNotBeNullOrWhiteSpace();
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task SaveAsync_ShouldHaveACheckSumOf8435_GivenLedgerBookTestData2()
    {
        var subject = CreateSubject(true);

        await subject.SaveAsync(LedgerBookTestData.TestData2(), "Foo.json", false);

        ExtractCheckSum(subject.SerialisedData).ShouldBe(8435.06);
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task SaveEncrypted_ShouldSaveTheJsonFile_GivenTestData2()
    {
        var fileName = @"CompleteSmellyFoo.json";

        var subject = CreateSubject(true);

        var testData = LedgerBookTestData.TestData2();
        await subject.SaveAsync(testData, fileName, true);

        subject.SerialisedData.ShouldNotBeNullOrWhiteSpace();
        this.stopwatch.Stop();
    }

    [Fact]
    public async Task SavingAndLoading_ShouldProduceTheSameCheckSum_GivenTestData2()
    {
        var subject1 = CreateSubject(true);
        await subject1.SaveAsync(LedgerBookTestData.TestData2(), "Foo2.json", false);
        var serialisedData = subject1.SerialisedData;

        // this.outputter.WriteLine("Saved / Serialised Json:");
        // this.outputter.WriteLine(serialisedData);

        var subject2 = CreateSubject();
        this.mockReaderWriterSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockReaderWriter);
        using var myStream = new MemoryStream(Encoding.UTF8.GetBytes(serialisedData));
        this.mockReaderWriter.CreateReadableStream(Arg.Any<string>()).Returns(myStream);
        await subject2.LoadAsync("foo", false);
        var bookDto = subject2.Dto;

        bookDto.Checksum.ShouldBe(ExtractCheckSum(serialisedData));
        this.stopwatch.Stop();
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
                new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), this.encryptedReaderWriter]), this.logger);
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
