using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class JsonOnDiskLedgerBookRepositoryTest
{
    private readonly IDtoMapper<LedgerBookDto, LedgerBook> mapper;
    private readonly IReaderWriterSelector mockReaderWriterSelector = Substitute.For<IReaderWriterSelector>();
    private readonly ITestOutputHelper output;

    public JsonOnDiskLedgerBookRepositoryTest(ITestOutputHelper output)
    {
        this.output = output;
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        var accountRepo = new InMemoryAccountTypeRepository();
        this.mapper = new MapperLedgerBookToDto2(
            bucketRepo,
            accountRepo,
            new LedgerBucketFactory(bucketRepo, accountRepo),
            new LedgerTransactionFactory());
    }

    [Fact]
    public async Task ConvertDemoFrom_DemoLedgerBookXml()
    {
        var xamlRepo = new XamlOnDiskLedgerBookRepositoryTestHarness(
            this.mapper,
            new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), new EmbeddedResourceFileReaderWriterEncrypted()]));
        var ledgerBook = await xamlRepo.LoadAsync(TestDataConstants.DemoLedgerBookFileName, false);

        var subject = CreateSubject();
        var mockReaderWriter = Substitute.For<IFileReaderWriter>();
        this.mockReaderWriterSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(mockReaderWriter);
        var myStream = new MemoryStream();
        mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);
        await subject.SaveAsync(ledgerBook, TestDataConstants.DemoLedgerBookFileName, false);

        this.output.WriteLine(subject.SerialisedData);
    }

    private JsonOnDiskLedgerBookRepositoryTestHarness CreateSubject(bool real = false)
    {
        var logger = new XUnitLogger(this.output);
        var importUtilities = new BankImportUtilitiesTestHarness(logger);
        if (real)
        {
            // Use real classes to operate very closely to live mode.
            return new JsonOnDiskLedgerBookRepositoryTestHarness(
                this.mapper,
                importUtilities,
                new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), new EmbeddedResourceFileReaderWriterEncrypted()]),
                logger);
        }

        // Use fake and mock objects where possible to better isolate testing.
        return new JsonOnDiskLedgerBookRepositoryTestHarness(
            this.mapper,
            importUtilities,
            this.mockReaderWriterSelector,
            logger);
    }
}
