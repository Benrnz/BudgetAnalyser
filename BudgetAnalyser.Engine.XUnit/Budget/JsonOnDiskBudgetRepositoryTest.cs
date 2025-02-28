using System.Diagnostics;
using System.Text.Json;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class JsonOnDiskBudgetRepositoryTest
{
    private readonly IReaderWriterSelector mockFileSelector;
    private readonly IFileReaderWriter mockReaderWriter;

    public JsonOnDiskBudgetRepositoryTest()
    {
        this.mockFileSelector = Substitute.For<IReaderWriterSelector>();
        this.mockReaderWriter = Substitute.For<IFileReaderWriter>();
        this.mockFileSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockReaderWriter);
    }

    [Fact]
    public async Task CreateNewShouldPopulateFileName()
    {
        var subject = Arrange();
        SetPrivateBudgetCollection(subject);
        var filename = "FooBar.xml";
        var collection = await subject.CreateNewAndSaveAsync(filename);
        collection.StorageKey.ShouldBe(filename);
    }

    [Fact]
    public async Task CreateNewShouldReturnCollectionWithOneBudgetInIt()
    {
        var subject = Arrange();
        SetPrivateBudgetCollection(subject);
        var filename = "FooBar.xml";
        var collection = await subject.CreateNewAndSaveAsync(filename);

        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task CreateNewShouldThrowGivenEmptyFileName()
    {
        var subject = Arrange();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(string.Empty));
    }

    [Fact]
    public async Task CreateNewShouldThrowGivenNullFileName()
    {
        var subject = Arrange();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(null));
    }

    [Fact]
    public async Task CreateNewShouldWriteToDisk()
    {
        var subject = Arrange();
        SetPrivateBudgetCollection(subject);
        var filename = "FooBar.xml";

        await subject.CreateNewAndSaveAsync(filename);

        this.mockFileSelector.Received().SelectReaderWriter(Arg.Any<bool>());
        await this.mockReaderWriter.Received().WriteToDiskAsync(Arg.Any<string>(), Arg.Any<string>());
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
            new XamlOnDiskBudgetRepository(
                null,
                new DtoMapperStub<BudgetCollectionDto, BudgetCollection>(),
                this.mockFileSelector));
    }

    [Fact]
    public async Task LoadShouldCallInitialiseOnTheBucketRepository()
    {
        var mockBucketRepository = Substitute.For<IBudgetBucketRepository>();
        var subject = Arrange();

        var data = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.BudgetCollectionTestDataFileName);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync(Arg.Any<string>()).Returns(data);

        await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName, false);

        mockBucketRepository.Received().Initialise(Arg.Any<IEnumerable<BudgetBucketDto>>());
    }

    [Fact]
    public async Task LoadShouldReturnACollectionAndSetFileName()
    {
        var subject = Arrange();
        var data = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.BudgetCollectionTestDataFileName);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync(Arg.Any<string>()).Returns(data);

        var collection = await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName, false);

        collection.StorageKey.ShouldBe(TestDataConstants.BudgetCollectionTestDataFileName);
    }

    [Fact]
    public async Task LoadShouldThrowIfAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
    {
        var subject = Arrange();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);

        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task LoadShouldThrowIfDeserialisedObjectIsNotBudgetCollection()
    {
        var subject = Arrange();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync(Arg.Any<string>()).Returns(string.Empty);

        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task LoadShouldThrowIfFileDoesntExist()
    {
        var subject = Arrange();
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task LoadShouldThrowIfFileFormatIsInvalid()
    {
        var subject = Arrange();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync(Arg.Any<string>()).Throws(new JsonException());

        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("SmellyPoo.xml", false));
    }

    [Fact]
    public async Task MustBeAbleToLoadDemoBudgetFile()
    {
        var subject = Arrange();
        var data = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.DemoBudgetFileName);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync(Arg.Any<string>()).Returns(data);


        var collection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileName, false);

        collection.StorageKey.ShouldBe(TestDataConstants.DemoBudgetFileName);
        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task MustBeAbleToLoadEmptyBudgetFile()
    {
        var subject = Arrange();
        var data = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.EmptyBudgetFileName);
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync(Arg.Any<string>()).Returns(data);

        var collection = await subject.LoadAsync(TestDataConstants.EmptyBudgetFileName, false);

        collection.StorageKey.ShouldBe(TestDataConstants.EmptyBudgetFileName);
        collection.Count.ShouldBe(1);
    }

    [Fact]
    public async Task SaveShouldNotThrowIfLoadHasntBeenCalled()
    {
        var subject = Arrange();
        await subject.SaveAsync();
    }

    [Fact]
    public async Task SaveShouldWriteToDisk()
    {
        var subject = Arrange();

        SetPrivateBudgetCollection(subject);

        await subject.SaveAsync();

        await this.mockReaderWriter.Received().WriteToDiskAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    private JsonOnDiskBudgetRepository Arrange(IBudgetBucketRepository bucketRepo = null)
    {
        if (bucketRepo is null)
        {
            bucketRepo = new InMemoryBudgetBucketRepository(new MapperBudgetBucketToDto2());
        }

        return new JsonOnDiskBudgetRepository(
            bucketRepo,
            new MapperBudgetCollectionToDto2(bucketRepo, new MapperBudgetModelToDto2(new MapperExpenseToDto2(bucketRepo), new MapperIncomeToDto2(bucketRepo))),
            this.mockFileSelector);
    }

    private static void SetPrivateBudgetCollection(JsonOnDiskBudgetRepository subject)
    {
        PrivateAccessor.SetField<JsonOnDiskBudgetRepository>(subject, "currentBudgetCollection", BudgetModelTestData.CreateCollectionWith1And2());
    }
}
