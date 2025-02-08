using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.Widgets.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Widgets;

public class XamlOnDiskWidgetRepositoryTest
{
    private readonly IFileReaderWriter mockReaderWriter = Substitute.For<IFileReaderWriter>();
    private readonly IReaderWriterSelector mockSelector = Substitute.For<IReaderWriterSelector>();
    private readonly IStandardWidgetCatalog mockWidgetCatalog = Substitute.For<IStandardWidgetCatalog>();
    private readonly ITestOutputHelper output;

    public XamlOnDiskWidgetRepositoryTest(ITestOutputHelper output)
    {
        this.output = output;
        this.mockSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockReaderWriter);
        this.mockWidgetCatalog.GetAll().Returns([new CurrentFileWidget()]);
    }

    [Fact]
    public async Task CreateShouldSerialiseAndWriteGivenFilename()
    {
        var subject = ArrangeUsingMocks();
        var serialised = string.Empty;
        this.mockReaderWriter.WriteToDiskAsync("foo.bar", Arg.Do<string>(s => serialised = s)).Returns(Task.CompletedTask);
        await subject.CreateNewAndSaveAsync("foo.bar");
        await this.mockReaderWriter.Received(1).WriteToDiskAsync("foo.bar", Arg.Any<string>());

        serialised.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task CreateShouldThrowGivenEmptyFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(string.Empty));
    }

    [Fact]
    public async Task CreateShouldThrowGivenNullFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(null!));
    }

    [Fact]
    public void CtorShouldThrowWhenGivenNullMapper()
    {
        Should.Throw<ArgumentNullException>(() => new XamlOnDiskWidgetRepository(null!, new XUnitLogger(this.output), this.mockSelector, new WidgetCatalog()));
    }

    [Fact]
    public async Task LoadFromDemoFileShouldReturnWidgets()
    {
        var subject = ArrangeUsingEmbeddedResources();
        var results = await subject.LoadAsync(TestDataConstants.TestDataWidgetsFileName, false);

        results.Any().ShouldBeTrue();
    }

    [Fact]
    public async Task LoadShouldThrowGivenBadFileFormat()
    {
        var subject = ArrangeUsingMocks();
        // If any exception is thrown during loading and parsing the file, it should be caught and rethrown as a DataFormatException.
        this.mockReaderWriter.FileExists("foo.bar").Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync("foo.bar").ThrowsAsync<Exception>();
        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("foo.bar", false));
    }

    [Fact]
    public async Task LoadShouldThrowGivenEmptyFileName()
    {
        var subject = ArrangeUsingMocks();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(false);
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync(string.Empty, false));
    }

    [Fact]
    public async Task LoadShouldThrowGivenNullFileName1()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync(null!, false));
    }

    [Fact]
    public async Task LoadShouldThrowGivenNullFileName2()
    {
        var subject = ArrangeUsingMocks();
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync(null!, false));
    }

    [Fact]
    public async Task LoadShouldThrowIfFileNotFound()
    {
        var subject = ArrangeUsingMocks();
        this.mockReaderWriter.FileExists("Foo.bar").Returns(false);
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync("Foo.bar", false));
    }

    [Fact]
    public async Task LoadShouldThrowIfLoadedNullFile()
    {
        var subject = ArrangeUsingMocks();
        this.mockReaderWriter.FileExists(Arg.Any<string>()).Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync("foo.bar").ReturnsNull();
        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("foo.bar", false));
    }

    [Fact]
    public async Task SaveShouldSerialiseAndWriteGivenValidModel()
    {
        var subject = ArrangeUsingMocks();
        var serialised = string.Empty;
        this.mockReaderWriter.WriteToDiskAsync("foo.bar", Arg.Do<string>(s => serialised = s)).Returns(Task.CompletedTask);
        var models = WidgetsTestData.ModelTestData1();
        await subject.SaveAsync(models, "foo.bar", false);
        await this.mockReaderWriter.Received(1).WriteToDiskAsync("foo.bar", Arg.Any<string>());

        serialised.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task SaveShouldThrowGivenNullFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.SaveAsync(WidgetsTestData.ModelTestData1(), null!, false));
    }

    [Fact]
    public async Task SaveShouldThrowGivenNullWidgetsList()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.SaveAsync(null!, "Foo.bar", false));
    }

    private XamlOnDiskWidgetRepository ArrangeUsingEmbeddedResources()
    {
        return new XamlOnDiskWidgetRepository(
            new MapperWidgetToDto(new WidgetCatalog()),
            new XUnitLogger(this.output),
            new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), new EmbeddedResourceFileReaderWriterEncrypted()]),
            new WidgetCatalog());
    }

    private XamlOnDiskWidgetRepository ArrangeUsingMocks()
    {
        return new XamlOnDiskWidgetRepository(new MapperWidgetToDto(this.mockWidgetCatalog), new XUnitLogger(this.output), this.mockSelector, new WidgetCatalog());
    }
}
