using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.Widgets.Data;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Widgets;

public class JsonOnDiskWidgetRepositoryTest
{
    private readonly IFileReaderWriter mockReaderWriter = Substitute.For<IFileReaderWriter>();
    private readonly IReaderWriterSelector mockSelector = Substitute.For<IReaderWriterSelector>();
    private readonly IStandardWidgetCatalog mockWidgetCatalog = Substitute.For<IStandardWidgetCatalog>();

    private readonly ITestOutputHelper output;
    private readonly EmbeddedResourceFileReaderWriterEncrypted encryptedReaderWriter = new();

    public JsonOnDiskWidgetRepositoryTest(ITestOutputHelper output)
    {
        this.output = output;
        this.mockSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockReaderWriter);
        this.mockWidgetCatalog.GetAll().Returns([new CurrentFileWidget()]);
    }

    [Fact]
    public async Task Create_ShouldSerialiseAndWrite_GivenFilename()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await subject.CreateNewAndSaveAsync("foo.bar");

        subject.SerialisedData.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Create_ShouldThrow_GivenEmptyFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(string.Empty));
    }

    [Fact]
    public async Task Create_ShouldThrow_GivenNullFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(null!));
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenGivenNullMapper()
    {
        Should.Throw<ArgumentNullException>(() => new XamlOnDiskWidgetRepository(null!, new XUnitLogger(this.output), this.mockSelector, new WidgetCatalog()));
    }

    // [Fact]
    // public async Task TempTest()
    // {
    //     var xamlRepo = new XamlOnDiskWidgetRepository(
    //         new MapperWidgetToDto(new WidgetCatalog()),
    //         new XUnitLogger(this.output),
    //         new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), this.encryptedReaderWriter]),
    //         new WidgetCatalog());
    //     var widgets = await xamlRepo.LoadAsync(@"BudgetAnalyser.Engine.XUnit.TestData.WidgetsTestData.xml", false);
    //     var jsonRepo = ArrangeUsingEmbeddedResources();
    //     await jsonRepo.SaveAsync(widgets, "foo.bar", false);
    //     this.output.WriteLine(jsonRepo.SerialisedData);
    // }

    [Fact]
    public async Task Load_ShouldReturnWidgets_GivenDemoFile()
    {
        var subject = ArrangeUsingEmbeddedResources();
        var results = await subject.LoadAsync(TestDataConstants.TestDataWidgetsFileName, false);

        results.Any().ShouldBeTrue();
        results.OfType<SurprisePaymentWidget>().First().StartPaymentDate.ShouldBe(new DateOnly(2013, 7, 1));
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenBadFileFormat()
    {
        var subject = ArrangeUsingMocks();
        // If any exception is thrown during loading and parsing the file, it should be caught and rethrown as a DataFormatException.
        this.mockReaderWriter.FileExists("foo.bar").Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync("foo.bar").ThrowsAsync<Exception>();
        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("foo.bar", false));
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenEmptyFileName()
    {
        var subject = ArrangeUsingMocks();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(false);
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync(string.Empty, false));
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenNullFileName1()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync(null!, false));
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenNullFileName2()
    {
        var subject = ArrangeUsingMocks();
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync(null!, false));
    }

    [Fact]
    public async Task Load_ShouldThrow_IfFileNotFound()
    {
        var subject = ArrangeUsingMocks();
        this.mockReaderWriter.FileExists("Foo.bar").Returns(false);
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync("Foo.bar", false));
    }

    [Fact]
    public async Task Load_ShouldThrow_IfLoadedNullFile()
    {
        var subject = ArrangeUsingMocks();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync("foo.bar").ReturnsNull();
        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("foo.bar", false));
    }

    [Fact]
    public async Task Save_ShouldSerialiseAndWrite_GivenValidModel()
    {
        var subject = ArrangeUsingEmbeddedResources();
        var models = WidgetsTestData.ModelTestData1();
        await subject.SaveAsync(models, "foo.bar", false);

        subject.SerialisedData.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Save_ShouldThrow_GivenNullFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.SaveAsync(WidgetsTestData.ModelTestData1(), null!, false));
    }

    [Fact]
    public async Task Save_ShouldThrow_GivenNullWidgetsList()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.SaveAsync(null!, "Foo.bar", false));
    }

    [Fact]
    public async Task LoadAndSave_ShouldProduceSameJson()
    {
        var subject = ArrangeUsingEmbeddedResources();
        var widgets = await subject.LoadAsync(TestDataConstants.TestDataWidgetsFileName, false);

        await subject.SaveAsync(widgets, "foo.bar", false);
        var serialisedData = JsonHelper.MinifyJson(subject.SerialisedData);
        var expectedData = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.TestDataWidgetsFileName);
        expectedData = JsonHelper.MinifyJson(expectedData);

        serialisedData.ShouldBe(expectedData);
    }

    private JsonOnDiskWidgetRepositoryTestHarness ArrangeUsingEmbeddedResources()
    {
        return new JsonOnDiskWidgetRepositoryTestHarness(new XUnitLogger(this.output), new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), this.encryptedReaderWriter]));
    }

    private JsonOnDiskWidgetRepositoryTestHarness ArrangeUsingMocks()
    {
        return new JsonOnDiskWidgetRepositoryTestHarness(new XUnitLogger(this.output), this.mockSelector);
    }
}
