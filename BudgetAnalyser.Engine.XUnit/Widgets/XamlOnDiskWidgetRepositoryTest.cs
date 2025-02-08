using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.Widgets.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
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
    public void CtorShouldThrowWhenGivenNullMapper()
    {
        Should.Throw<ArgumentNullException>(() => new XamlOnDiskWidgetRepository(null, new XUnitLogger(this.output), this.mockSelector));
    }

    [Fact]
    public async Task LoadFromDemoFileShouldReturnWidgets()
    {
        var subject = ArrangeUsingEmbeddedResources();
        var results = await subject.LoadAsync(TestDataConstants.TestDataWidgetsFileName, false);

        results.ShouldNotBeNull();
        results.Any().ShouldBeTrue();
    }

    [Fact]
    public async Task LoadShouldReturnWidgets()
    {
        var subject = ArrangeUsingEmbeddedResources();
        subject.LoadFromDiskOveride = fileName => WidgetsTestData.RawDtoTestData1().ToList();
        var results = await subject.LoadAsync("foo.bar", false);

        results.ShouldNotBeNull();
        results.Any().ShouldBeTrue();
    }

    [Fact]
    public async Task LoadShouldThrowGivenBadFileFormat()
    {
        var subject = ArrangeUsingEmbeddedResources();
        subject.LoadFromDiskOveride = fileName => throw new Exception();
        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("foo.bar", false));
    }

    [Fact]
    public async Task LoadShouldThrowGivenNullFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync(null, false));
    }

    [Fact]
    public async Task LoadShouldThrowIfFileNotFound()
    {
        var subject = ArrangeUsingMocks();
        subject.ExistsOveride = filename => false;
        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync("Foo.bar", false));
    }

    [Fact]
    public async Task LoadShouldThrowIfLoadedNullFile()
    {
        var subject = ArrangeUsingEmbeddedResources();
        subject.LoadFromDiskOveride = fileName => null;
        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("foo.bar", false));
    }

    [Fact]
    public async Task SaveShouldThrowGivenNullFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.SaveAsync(WidgetsTestData.ModelTestData1(), null, false));
    }

    [Fact]
    public async Task SaveShouldThrowGivenNullWidgetsList()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.SaveAsync(null, "Foo.bar", false));
    }

    [Fact]
    public async Task SaveTest()
    {
        var subject = ArrangeUsingMocks();
        var models = WidgetsTestData.ModelTestData1();
        await subject.SaveAsync(models, "foo.bar", false);
        await this.mockReaderWriter.Received(1).WriteToDiskAsync("foo.bar", Arg.Any<string>());

        subject.SerialisedData.ShouldNotBeEmpty();
    }

    private XamlOnDiskWidgetRepositoryTestHarness ArrangeUsingEmbeddedResources()
    {
        return new XamlOnDiskWidgetRepositoryTestHarness(
            new MapperWidgetToDto(new WidgetCatalog()),
            new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), new EmbeddedResourceFileReaderWriterEncrypted()]),
            this.output);
    }

    private XamlOnDiskWidgetRepositoryTestHarness ArrangeUsingMocks()
    {
        return new XamlOnDiskWidgetRepositoryTestHarness(new MapperWidgetToDto(this.mockWidgetCatalog), this.mockSelector, this.output);
    }
}
