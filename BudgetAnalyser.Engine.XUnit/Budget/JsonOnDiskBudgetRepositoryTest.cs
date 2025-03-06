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
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class JsonOnDiskBudgetRepositoryTest
{
    private readonly BucketBucketRepoAlwaysFind bucketRepo = new();
    private readonly EmbeddedResourceFileReaderWriterEncrypted encryptedReaderWriter = new();
    private readonly ILogger logger;
    private readonly IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper;
    private readonly IReaderWriterSelector mockFileSelector;
    private readonly IFileReaderWriter mockReaderWriter;
    private readonly XUnitOutputWriter outputter;

    public JsonOnDiskBudgetRepositoryTest(ITestOutputHelper output)
    {
        this.outputter = new XUnitOutputWriter(output);
        this.logger = new XUnitLogger(output);
        this.mockFileSelector = Substitute.For<IReaderWriterSelector>();
        this.mockReaderWriter = Substitute.For<IFileReaderWriter>();
        this.mockFileSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockReaderWriter);
        this.mapper = new MapperBudgetCollectionToDto2(this.bucketRepo, new MapperBudgetModelToDto2(new MapperExpenseToDto2(this.bucketRepo), new MapperIncomeToDto2(this.bucketRepo)));
    }

    [Fact]
    public async Task CreateNewShouldPopulateFileName()
    {
        var subject = CreateSubject();
        var filename = "FooBar.xml";
        using var myStream = new MemoryStream();
        this.mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);
        var collection = await subject.CreateNewAndSaveAsync(filename);
        collection.StorageKey.ShouldBe(filename);
    }

    [Fact]
    public async Task CreateNewShouldReturnCollectionWithOneBudgetInIt()
    {
        var subject = CreateSubject();
        var filename = "FooBar.xml";
        using var myStream = new MemoryStream();
        this.mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);
        var collection = await subject.CreateNewAndSaveAsync(filename);
        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task CreateNewShouldThrowGivenEmptyFileName()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(string.Empty));
    }

    [Fact]
    public async Task CreateNewShouldThrowGivenNullFileName()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(null));
    }

    [Fact]
    public async Task CreateNewShouldWriteToDisk()
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
    public async Task CtorShouldThrowGivenNullMapper()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            new XamlOnDiskBudgetRepository(
                new BucketBucketRepoAlwaysFind(),
                null,
                this.mockFileSelector));
    }

    [Fact]
    public async Task CtorShouldThrowWhenBucketRepositoryIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            new JsonOnDiskBudgetRepository(
                null,
                this.mapper,
                this.mockFileSelector));
    }

    [Fact]
    public async Task LoadShouldCallInitialiseOnTheBucketRepository()
    {
        var mockBucketRepository = Substitute.For<IBudgetBucketRepository>();

        var subject = CreateSubject(true, mockBucketRepository);

        var budgetCollection = await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName, false);

        budgetCollection.ShouldNotBeNull();
        mockBucketRepository.Received().Initialise(Arg.Any<IEnumerable<BudgetBucketDto>>());
    }

    // [Fact]
    // public async Task TempTest()
    // {
    //     var xamlRepo = new XamlOnDiskBudgetRepository(this.bucketRepo, this.mapper, new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), this.encryptedReaderWriter]));
    //     var budgetCollection = await xamlRepo.LoadAsync(TestDataConstants.EmptyBudgetFileName, false);
    //
    //     var subject = CreateSubject(true);
    //
    //     await subject.SaveAsync(TestDataConstants.EmptyBudgetFileName, false);
    //
    //     subject.SerialisedData.ShouldNotBeNullOrEmpty();
    //     this.outputter.WriteLine(subject.SerialisedData);
    // }

    [Fact]
    public async Task LoadShouldReturnACollectionAndSetFileName()
    {
        var subject = CreateSubject(true);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);

        var collection = await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName, false);

        collection.StorageKey.ShouldBe(TestDataConstants.BudgetCollectionTestDataFileName);
    }

    [Fact]
    public async Task LoadShouldThrowIfAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
    {
        var subject = CreateSubject();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);

        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task LoadShouldThrowIfDeserialisedObjectIsNotBudgetCollection()
    {
        var subject = CreateSubject();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync(Arg.Any<string>()).Returns(string.Empty);

        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task LoadShouldThrowIfFileDoesntExist()
    {
        var subject = CreateSubject();
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task LoadShouldThrowIfFileFormatIsInvalid()
    {
        var subject = CreateSubject();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync(Arg.Any<string>()).Throws(new JsonException());

        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task MustBeAbleToLoadDemoBudgetFile()
    {
        var subject = CreateSubject(true);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);

        var collection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileNameJson, false);

        collection.StorageKey.ShouldBe(TestDataConstants.DemoBudgetFileNameJson);
        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task MustBeAbleToLoadEmptyBudgetFile()
    {
        var subject = CreateSubject(true);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);

        var collection = await subject.LoadAsync(TestDataConstants.EmptyBudgetFileName, false);

        collection.StorageKey.ShouldBe(TestDataConstants.EmptyBudgetFileName);
        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task SaveShouldNotThrowIfLoadHasntBeenCalled()
    {
        using var myStream = new MemoryStream();
        this.mockReaderWriter.CreateWritableStream(Arg.Any<string>()).Returns(myStream);

        var subject = CreateSubject();
        await subject.SaveAsync();
    }

    [Fact]
    public async Task SaveShouldWriteToDisk()
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
