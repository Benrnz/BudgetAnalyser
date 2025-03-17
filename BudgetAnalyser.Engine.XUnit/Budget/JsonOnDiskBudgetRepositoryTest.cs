using System.Text;
using System.Text.Json;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Rees.UnitTestUtilities;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class JsonOnDiskBudgetRepositoryTest : IDisposable
{
    private readonly BudgetBucketRepoAlwaysFind bucketRepo = new();
    private readonly EmbeddedResourceFileReaderWriterEncrypted encryptedReaderWriter = new();
    private readonly ILogger logger;
    private readonly IReaderWriterSelector mockFileSelector;
    private readonly IFileReaderWriter mockReaderWriter;
    private readonly XUnitOutputWriter outputter;
    private IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper;

    public JsonOnDiskBudgetRepositoryTest(ITestOutputHelper output)
    {
        this.outputter = new XUnitOutputWriter(output);
        this.logger = new XUnitLogger(output);
        this.mockFileSelector = Substitute.For<IReaderWriterSelector>();
        this.mockReaderWriter = Substitute.For<IFileReaderWriter>();
        this.mockFileSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockReaderWriter);
        this.mapper = new MapperBudgetCollectionToDto2(this.bucketRepo, new MapperBudgetModelToDto2(new MapperExpenseToDto2(this.bucketRepo), new MapperIncomeToDto2(this.bucketRepo)));
    }

    public void Dispose()
    {
        this.outputter?.Dispose();
        this.encryptedReaderWriter.InputStream?.Dispose();
        this.encryptedReaderWriter.OutputStream?.Dispose();
    }

    [Fact]
    public async Task CreateNew_ShouldPopulateFileName()
    {
        var subject = CreateSubject();
        var filename = "FooBar.xml";
        using var myStream = new MemoryStream();
        this.mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);
        var collection = await subject.CreateNewAndSaveAsync(filename);
        collection.StorageKey.ShouldBe(filename);
    }

    [Fact]
    public async Task CreateNew_ShouldReturnCollectionThatIsValid()
    {
        var subject = CreateSubject();
        var filename = "FooBar.xml";
        using var myStream = new MemoryStream();
        this.mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);
        var collection = await subject.CreateNewAndSaveAsync(filename);
        collection.Validate(new StringBuilder()).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateNew_ShouldReturnCollectionWithOneBudgetInIt()
    {
        var subject = CreateSubject();
        var filename = "FooBar.xml";
        using var myStream = new MemoryStream();
        this.mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);
        var collection = await subject.CreateNewAndSaveAsync(filename);
        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task CreateNew_ShouldThrow_GivenEmptyFileName()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(string.Empty));
    }

    [Fact]
    public async Task CreateNew_ShouldThrow_GivenNullFileName()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(null));
    }

    [Fact]
    public async Task CreateNew_ShouldWriteToDisk()
    {
        var subject = CreateSubject();
        var filename = "FooBar.xml";
        using var myStream = new MemoryStream();
        this.mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);

        await subject.CreateNewAndSaveAsync(filename);

        subject.SerialisedData.ShouldNotBeNullOrEmpty();
        subject.SerialisedData.Length.ShouldBeGreaterThan(100);
    }

    [Fact]
    public async Task Ctor_ShouldThrow_GivenNullMapper()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            new JsonOnDiskBudgetRepository(
                new BudgetBucketRepoAlwaysFind(),
                null,
                this.mockFileSelector));
    }

    [Fact]
    public async Task Ctor_ShouldThrow_WhenBucketRepositoryIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            new JsonOnDiskBudgetRepository(
                null,
                this.mapper,
                this.mockFileSelector));
    }

    [Fact]
    public async Task Load_ShouldCallInitialiseOnTheBucketRepository()
    {
        var mockBucketRepository = Substitute.For<IBudgetBucketRepository>();

        var subject = CreateSubject(true, mockBucketRepository);

        var budgetCollection = await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName, false);

        budgetCollection.ShouldNotBeNull();
        mockBucketRepository.Received().Initialise(Arg.Any<IEnumerable<BudgetBucketDto>>());
    }

    [Fact]
    public async Task Load_ShouldLoad_GivenDemoBudgetFile()
    {
        var subject = CreateSubject(true);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);

        var collection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileName, false);

        collection.StorageKey.ShouldBe(TestDataConstants.DemoBudgetFileName);
        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Load_ShouldLoad_GivenEmptyBudgetFile()
    {
        var subject = CreateSubject(true);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);

        var collection = await subject.LoadAsync(TestDataConstants.EmptyBudgetFileName, false);

        collection.StorageKey.ShouldBe(TestDataConstants.EmptyBudgetFileName);
        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Load_ShouldLoad_GivenEncryptedDemoBudgetFile()
    {
        var subject = CreateSubject(true);
        this.encryptedReaderWriter.Password = TestDataConstants.DemoEncryptedFilePassword;
        var collection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileNameEncrypted, true);
        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Load_ShouldReturnACollectionAndSetFileName()
    {
        var subject = CreateSubject(true);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);

        var collection = await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName, false);

        collection.StorageKey.ShouldBe(TestDataConstants.BudgetCollectionTestDataFileName);
    }

    [Fact]
    public async Task Load_ShouldReturnValidCollection_GivenDemoBudgetFile()
    {
        var subject = CreateSubject(true);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);

        var collection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileName, false);

        collection.Validate(new StringBuilder()).ShouldBeTrue();
    }

    [Fact]
    public async Task Load_ShouldThrow_WhenAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
    {
        var subject = CreateSubject();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);

        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task Load_ShouldThrow_WhenDeserialisedObjectIsNotBudgetCollection()
    {
        var subject = CreateSubject();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync(Arg.Any<string>()).Returns(string.Empty);

        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task Load_ShouldThrow_WhenFileDoesntExist()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task Load_ShouldThrow_WhenFileFormatIsInvalid()
    {
        var subject = CreateSubject();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync(Arg.Any<string>()).Throws(new JsonException());

        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task LoadAndSave_ShouldCreateReadableEncyptedData_GivenEncryptedDemoBudgetFile()
    {
        // Read in demo encrypted binary static test data
        var subject = CreateSubject(true);
        this.encryptedReaderWriter.Password = TestDataConstants.DemoEncryptedFilePassword;
        await subject.LoadAsync(TestDataConstants.DemoBudgetFileNameEncrypted, true);

        // Write out the collection to a new encrypted binary stream.
        await subject.SaveAsync(TestDataConstants.DemoBudgetFileNameEncrypted, true);
        using var binaryStream = new MemoryStream(subject.SerialisedBytes);

        // Try to read that stream back in to prove the save method is creating readable/reversible encrypted data.
        this.encryptedReaderWriter.InputStream = binaryStream;
        var collection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileNameEncrypted, true);
        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task LoadAndSave_ShouldProduceSameJson()
    {
        var subject = CreateSubject(true);
        await subject.LoadAsync(TestDataConstants.DemoBudgetFileName, false);

        await subject.SaveAsync(TestDataConstants.DemoBudgetFileName, false);
        var serialisedData = JsonHelper.MinifyJson(subject.SerialisedData);
        var expectedData = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.DemoBudgetFileName);
        expectedData = JsonHelper.MinifyJson(expectedData);

        serialisedData.ShouldBe(expectedData);
    }

    [Fact]
    public async Task LoadAndSave_ShouldRecreateSameBudgetCollection_GivenEncryptedDemoBudgetFile()
    {
        // Arrange
        // This test needs a full InMemoryBudgetBucketRepository to be able to recreate the same collection, the AlwaysFind test repo takes shortcuts.
        var bucketRepo2 = new InMemoryBudgetBucketRepository(new MapperBudgetBucketToDto2());
        this.mapper = new MapperBudgetCollectionToDto2(bucketRepo2, new MapperBudgetModelToDto2(new MapperExpenseToDto2(bucketRepo2), new MapperIncomeToDto2(bucketRepo2)));
        this.encryptedReaderWriter.Password = TestDataConstants.DemoEncryptedFilePassword;

        //Act - Save the loaded collection to generate an output stream and bytes.
        var subject = CreateSubject(true, bucketRepo2);
        var sourceCollection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileNameEncrypted, true);
        await subject.SaveAsync(TestDataConstants.DemoBudgetFileNameEncrypted, true);

        // Act - Reread the outputted stream and bytes to prove the save wrote the data correctly so it can be reloaded.
        using var rereadStream = new MemoryStream(subject.SerialisedBytes);
        this.encryptedReaderWriter.InputStream = rereadStream;
        var rereadCollection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileNameEncrypted, true);

        rereadCollection.Count.ShouldBe(sourceCollection.Count);

        for (var index = 0; index < sourceCollection.Count; index++)
        {
            var source = sourceCollection[index];
            var reread = rereadCollection[index];
            reread.EffectiveFrom.ShouldBe(source.EffectiveFrom);
            reread.Name.ShouldBe(source.Name);
            reread.Surplus.ShouldBe(source.Surplus);
            reread.BudgetCycle.ShouldBe(source.BudgetCycle);
            reread.LastModified.ShouldBe(source.LastModified);
            reread.LastModifiedComment.ShouldBe(source.LastModifiedComment);
            reread.Expenses.Count().ShouldBe(source.Expenses.Count());
            reread.Incomes.Count().ShouldBe(source.Incomes.Count());
            for (var expenseIndex = 0; expenseIndex < source.Expenses.Count(); expenseIndex++)
            {
                var sourceExpense = source.Expenses.ElementAt(expenseIndex);
                var rereadExpense = reread.Expenses.ElementAt(expenseIndex);
                rereadExpense.Amount.ShouldBe(sourceExpense.Amount);
                rereadExpense.Bucket.Code.ShouldBe(sourceExpense.Bucket.Code);
                rereadExpense.Bucket.GetType().ShouldBe(sourceExpense.Bucket.GetType());
            }

            for (var incomeIndex = 0; incomeIndex < source.Incomes.Count(); incomeIndex++)
            {
                var sourceExpense = source.Incomes.ElementAt(incomeIndex);
                var rereadExpense = reread.Incomes.ElementAt(incomeIndex);
                rereadExpense.Amount.ShouldBe(sourceExpense.Amount);
                rereadExpense.Bucket.Code.ShouldBe(sourceExpense.Bucket.Code);
                rereadExpense.Bucket.GetType().ShouldBe(sourceExpense.Bucket.GetType());
            }
        }
    }

    [Fact]
    public async Task Save_ShouldSave_WhenLoadHasntBeenCalled()
    {
        using var myStream = new MemoryStream();
        this.mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);

        var subject = CreateSubject();
        await subject.SaveAsync();
    }

    [Fact]
    public async Task Save_ShouldWriteToDisk_GivenNewModel()
    {
        using var myStream = new MemoryStream();
        this.mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);

        var subject = CreateSubject();

        await subject.SaveAsync();

        subject.SerialisedData.ShouldNotBeNullOrEmpty();
    }

    private JsonOnDiskBudgetRepositoryTestHarness CreateSubject(bool real = false, IBudgetBucketRepository bucketRepo = null)
    {
        bucketRepo ??= this.bucketRepo;
        if (real)
        {
            // Use real classes to operate very closely to live mode.
            return new JsonOnDiskBudgetRepositoryTestHarness(
                bucketRepo,
                this.mapper,
                new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), this.encryptedReaderWriter]));
        }

        // Use fake and mock objects where possible to better isolate testing.
        return new JsonOnDiskBudgetRepositoryTestHarness(
            bucketRepo,
            this.mapper,
            this.mockFileSelector);
    }
}
