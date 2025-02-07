using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.Widgets.Data;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Moq;
using NSubstitute;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Widgets;

public class XamlOnDiskWidgetRepositoryTest
{
    private readonly IFileReaderWriter mockReaderWriter = Substitute.For<IFileReaderWriter>();
    private readonly IReaderWriterSelector mockSelector = Substitute.For<IReaderWriterSelector>();
    private readonly IWidgetRepository mockWidgetRepo = Substitute.For<IWidgetRepository>();
    private readonly ITestOutputHelper output;

    public XamlOnDiskWidgetRepositoryTest(ITestOutputHelper output)
    {
        this.output = output;

        this.mockSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockReaderWriter);
        this.mockWidgetRepo.GetAll().Returns([new CurrentFileWidget()]);
    }

    [Fact]
    public void CtorShouldThrowWhenGivenNullMapper()
    {
        new XamlOnDiskWidgetRepository(null, new XUnitLogger(this.output), new Mock<IReaderWriterSelector>().Object);
        Assert.Fail();
    }

    [Fact]
    public async Task LoadFromDemoFileShouldReturnMatchingRules()
    {
        var subject = ArrangeUsingEmbeddedResources();
        var results = await subject.LoadAsync(TestDataConstants.DemoRulesFileName, false);

        results.ShouldNotBeNull();
        results.Any().ShouldBeTrue();
    }

    [Fact]
    public async Task LoadShouldReturnMatchingRules()
    {
        var subject = ArrangeUsingEmbeddedResources();
        subject.LoadFromDiskOveride = fileName => MatchingRulesTestData.RawTestData1().ToList();
        var results = await subject.LoadAsync("foo.bar", false);

        results.ShouldNotBeNull();
        results.Any().ShouldBeTrue();
    }

    [Fact]
    //[ExpectedException(typeof(DataFormatException))]
    public async Task LoadShouldThrowGivenBadFileFormat()
    {
        var subject = ArrangeUsingEmbeddedResources();
        subject.LoadFromDiskOveride = fileName => { throw new Exception(); };
        await subject.LoadAsync("foo.bar", false);
        Assert.Fail();
    }

    [Fact]
    //[ExpectedException(typeof(KeyNotFoundException))]
    public async Task LoadShouldThrowGivenNullFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await subject.LoadAsync(null, false);
        Assert.Fail();
    }

    [Fact]
    //[ExpectedException(typeof(KeyNotFoundException))]
    public async Task LoadShouldThrowIfFileNotFound()
    {
        var subject = ArrangeUsingMocks();
        subject.ExistsOveride = filename => false;
        await subject.LoadAsync("Foo.bar", false);
        Assert.Fail();
    }

    [Fact]
    //[ExpectedException(typeof(DataFormatException))]
    public async Task LoadShouldThrowIfLoadedNullFile()
    {
        var subject = ArrangeUsingEmbeddedResources();
        subject.LoadFromDiskOveride = fileName => null;
        await subject.LoadAsync("foo.bar", false);
        Assert.Fail();
    }

    [Fact]
    //[ExpectedException(typeof(ArgumentNullException))]
    public async Task SaveShouldThrowGivenNullFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await subject.SaveAsync(MatchingRulesTestDataGenerated.TestData1(), null, false);
        Assert.Fail();
    }

    [Fact]
    //[ExpectedException(typeof(ArgumentNullException))]
    public async Task SaveShouldThrowGivenNullRulesList()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await subject.SaveAsync(null, "Foo.bar", false);
        Assert.Fail();
    }

    private XamlOnDiskWidgetRepositoryTestHarness ArrangeUsingEmbeddedResources()
    {
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        MatchingRulesTestDataGenerated.BucketRepo = bucketRepo;

        return new XamlOnDiskWidgetRepositoryTestHarness(
            new WidgetToDtoMapper(this.mockWidgetRepo),
            new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), new EmbeddedResourceFileReaderWriterEncrypted()]),
            this.output);
    }

    private XamlOnDiskWidgetRepositoryTestHarness ArrangeUsingMocks()
    {
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        MatchingRulesTestDataGenerated.BucketRepo = bucketRepo;

        return new XamlOnDiskWidgetRepositoryTestHarness(new WidgetToDtoMapper(this.mockWidgetRepo), this.mockSelector, this.output);
    }
}
