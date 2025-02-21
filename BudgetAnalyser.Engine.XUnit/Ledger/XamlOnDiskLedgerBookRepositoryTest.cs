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
using Moq;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class XamlOnDiskLedgerBookRepositoryTest : IDisposable
{
    private const string LoadFileName = @"BudgetAnalyser.Engine.XUnit.TestData.LedgerBookRepositoryTest_Load_ShouldLoadTheXmlFile.xml";
    private readonly XUnitOutputWriter outputter;

    private readonly IDtoMapper<LedgerBookDto, LedgerBook> mapper;
    private readonly Mock<IFileReaderWriter> mockReaderWriter;
    private readonly Mock<IReaderWriterSelector> mockReaderWriterSelector;
    private readonly Stopwatch stopwatch;

    public XamlOnDiskLedgerBookRepositoryTest(ITestOutputHelper output)
    {
        this.outputter = new XUnitOutputWriter(output);
        var accountRepo = new InMemoryAccountTypeRepository();
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        this.mapper = new MapperLedgerBookToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory());
        this.mockReaderWriterSelector = new Mock<IReaderWriterSelector>();
        this.mockReaderWriter = new Mock<IFileReaderWriter>();
        this.mockReaderWriterSelector.Setup(m => m.SelectReaderWriter(It.IsAny<bool>())).Returns(this.mockReaderWriter.Object);

        this.stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        this.outputter?.WriteLine($"TOTAL TIME: {this.stopwatch.Elapsed}");
        this.outputter?.Dispose();
    }

    [Fact]
    public async Task DemoBookFileChecksum_ShouldNotChange_WhenLoadAndSave()
    {
        double fileChecksum = 0;
        var subject = CreateSubject(true);
        LedgerBookDto predeserialiseDto = null;

        subject.DtoDeserialised += (s, e) =>
        {
            fileChecksum = subject.LedgerBookDto.Checksum;
            subject.LedgerBookDto.Checksum = -1;
            predeserialiseDto = subject.LedgerBookDto;
        };

        LedgerBookDto reserialisedDto = null;
        subject.SaveDtoToDiskOverride = bookDto => reserialisedDto = bookDto;

        var book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName, false);
        predeserialiseDto.Output(true, this.outputter);

        await subject.SaveAsync(book, book.StorageKey, false);

        reserialisedDto.Output(true, this.outputter);

        reserialisedDto.Checksum.ShouldBe(fileChecksum);

        this.stopwatch.Stop();
    }

    [Fact]
    public async Task Load_Output()
    {
        XamlOnDiskLedgerBookRepository subject = CreateSubject();
        var book = await subject.LoadAsync(LoadFileName, false);

        // Visual compare these two - should be the same
        LedgerBookTestData.TestData2().Output(outputWriter: this.outputter);

        book.Output(outputWriter: this.outputter);
    }

    [Fact]
    public async Task Load_ShouldCreateBookThatIsValid()
    {
        XamlOnDiskLedgerBookRepository subject = CreateSubject();
        var book = await subject.LoadAsync(LoadFileName, false);
        var builder = new StringBuilder();
        book.Validate(builder).ShouldBeTrue(builder.ToString());
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithFirstLineEqualBankBalances()
    {
        XamlOnDiskLedgerBookRepository subject = CreateSubject();
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();
        var line = book.Reconciliations.First();

        line.TotalBankBalance.ShouldBe(testData2.Reconciliations.First().TotalBankBalance);
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithFirstLineEqualSurplus()
    {
        XamlOnDiskLedgerBookRepository subject = CreateSubject();
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
        XamlOnDiskLedgerBookRepository subject = CreateSubject();
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Modified.ShouldBe(testData2.Modified);
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameName()
    {
        XamlOnDiskLedgerBookRepository subject = CreateSubject();
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Name.ShouldBe(testData2.Name);
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameNumberOfLedgers()
    {
        XamlOnDiskLedgerBookRepository subject = CreateSubject();
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Ledgers.Count().ShouldBe(testData2.Ledgers.Count());
    }

    [Fact]
    public async Task Load_ShouldCreateBookWithSameNumberOfReconciliations()
    {
        XamlOnDiskLedgerBookRepository subject = CreateSubject();
        var book = await subject.LoadAsync(LoadFileName, false);
        var testData2 = LedgerBookTestData.TestData2();

        book.Reconciliations.Count().ShouldBe(testData2.Reconciliations.Count());
    }

    [Fact]
    public async Task Load_ShouldLoadTheXmlFile()
    {
        XamlOnDiskLedgerBookRepository subject = CreateSubject();
        var book = await subject.LoadAsync(LoadFileName, false);

        book.ShouldNotBeNull();
    }

    [Fact]
    public async Task MustBeAbleToLoadDemoLedgerBookFile()
    {
        XamlOnDiskLedgerBookRepository subject = CreateSubject();

        var book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName, false);
        book.Output(true, this.outputter);
        book.ShouldNotBeNull();
    }

    [Fact]
    public async Task Save_ShouldSaveTheXmlFile()
    {
        var fileName = @"CompleteSmellyFoo.xml";

        XamlOnDiskLedgerBookRepository subject = CreateSubject();

        var testData = LedgerBookTestData.TestData2();
        await subject.SaveAsync(testData, fileName, false);

        this.mockReaderWriter.Verify(m => m.WriteToDiskAsync(It.IsAny<string>(), It.IsAny<string>()));
    }

    [Fact]
    public async Task SaveAsync_ShouldHaveACheckSumOf8435_GivenLedgerBookTestData2()
    {
        var subject = CreateSubject();

        await subject.SaveAsync(LedgerBookTestData.TestData2(), "Foo.xml", false);

        var serialisedData = subject.SerialisedData;
        var checksumPosition = serialisedData.IndexOf("CheckSum=\"", StringComparison.OrdinalIgnoreCase);
        var checksumLength = serialisedData.IndexOf('"', checksumPosition + 11) - checksumPosition;
        var serialisedCheckSum = serialisedData.Substring(checksumPosition + 10, checksumLength - 10);

        double.Parse(serialisedCheckSum).ShouldBe(8435.06);
    }

    [Fact]
    public async Task SavingAndLoading_ShouldProduceTheSameCheckSum()
    {
        var subject1 = CreateSubject();

        await subject1.SaveAsync(LedgerBookTestData.TestData2(), "Foo2.xml", false);
        var serialisedData = subject1.SerialisedData;

        Debug.WriteLine("Saved / Serialised Xml:");
        Debug.WriteLine(serialisedData);

        LedgerBookDto bookDto;
        var subject2 = CreateSubject();
        subject2.FileExistsOverride = f => true;
        subject2.LoadXamlFromDiskFromEmbeddedResources = false;
        this.mockReaderWriter.Setup(m => m.LoadFromDiskAsync(It.IsAny<string>())).ReturnsAsync(serialisedData);
        await subject2.LoadAsync("foo", false);
        bookDto = subject2.LedgerBookDto;

        var checksumPosition = serialisedData.IndexOf("CheckSum=\"", StringComparison.OrdinalIgnoreCase);
        var checksumLength = serialisedData.IndexOf('"', checksumPosition + 11) - checksumPosition;
        var serialisedCheckSum = serialisedData.Substring(checksumPosition + 10, checksumLength - 10);

        double.Parse(serialisedCheckSum).ShouldBe(bookDto.Checksum);
    }

    [Fact]
    public async Task SerialiseTestData2ToEnsureItMatches_Load_ShouldLoadTheXmlFile_xml()
    {
        var subject = CreateSubject();

        await subject.SaveAsync(LedgerBookTestData.TestData2(), "Leonard Nimoy.xml", false);
        var serialisedData = subject.SerialisedData;

        this.outputter.WriteLine(serialisedData);

        serialisedData.Length.ShouldBeGreaterThan(100);
    }

    private XamlOnDiskLedgerBookRepositoryTestHarness CreateSubject(bool real = false)
    {
        if (real)
        {
            // Use real classes to operation very closely to live mode.
            return new XamlOnDiskLedgerBookRepositoryTestHarness(
                this.mapper,
                new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), new EmbeddedResourceFileReaderWriterEncrypted()]));
        }

        // Use fake and mock objects where possible to better isolate testing.
        return new XamlOnDiskLedgerBookRepositoryTestHarness(
            this.mapper,
            this.mockReaderWriterSelector.Object);
    }
}
