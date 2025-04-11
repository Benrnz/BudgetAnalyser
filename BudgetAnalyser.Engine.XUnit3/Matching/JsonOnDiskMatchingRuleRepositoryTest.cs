using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Matching;

public class JsonOnDiskMatchingRuleRepositoryTest
{
    private readonly IFileReaderWriter mockReaderWriter = Substitute.For<IFileReaderWriter>();
    private readonly IReaderWriterSelector mockSelector = Substitute.For<IReaderWriterSelector>();
    private readonly IBudgetBucketRepository bucketRepo = new BudgetBucketRepoAlwaysFind();
    private readonly ITestOutputHelper output;
    private readonly EmbeddedResourceFileReaderWriterEncrypted encryptedReaderWriter = new();

    public JsonOnDiskMatchingRuleRepositoryTest(ITestOutputHelper output)
    {
        this.output = output;
        this.mockSelector.SelectReaderWriter(Arg.Any<bool>()).Returns(this.mockReaderWriter);
    }

    // [Fact]
    // public async Task TempTest()
    // {
    //     var xamlRepo = new XamlOnDiskMatchingRuleRepository(
    //         new MapperMatchingRuleToDto2(this.bucketRepo),
    //         new XUnitLogger(this.output),
    //         new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), this.encryptedReaderWriter]));
    //     var rules = await xamlRepo.LoadAsync(TestDataConstants.DemoRulesFileName, false);
    //
    //     var subject = ArrangeUsingEmbeddedResources();
    //     await subject.SaveAsync(rules, TestDataConstants.DemoRulesFileName, false);
    //     this.output.WriteLine(subject.SerialisedData);
    // }

    [Fact]
    public async Task Create_ShouldSerialiseAndWrite_GivenFilename()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await subject.CreateNewAndSaveAsync("rules.json");

        subject.SerialisedData.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Create_ShouldThrow_GivenEmptyFileName()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.CreateNewAndSaveAsync(string.Empty));
    }

    [Fact]
    public async Task Load_ShouldReturnMatchingRules_GivenValidFile()
    {
        var subject = ArrangeUsingEmbeddedResources();
        var results = await subject.LoadAsync(TestDataConstants.DemoRulesFileName, false);

        results.ShouldNotBeNull();
        results.ShouldBeOfType<List<MatchingRule>>();
    }

    [Fact]
    public async Task Load_ShouldThrow_GivenBadFileFormat()
    {
        var subject = ArrangeUsingMocks();
        this.mockReaderWriter.FileExists("bad-format.json").Returns(true);
        this.mockReaderWriter.LoadFromDiskAsync("bad-format.json").ThrowsAsync<Exception>();

        await Should.ThrowAsync<DataFormatException>(async () => await subject.LoadAsync("bad-format.json", false));
    }

    [Fact]
    public async Task Save_ShouldSerialiseAndWrite_GivenValidRules()
    {
        var subject = ArrangeUsingEmbeddedResources();
        var rules = new List<MatchingRule>
        {
            new(this.bucketRepo) { Description = "Test Rule", Reference1 = "123" }
        };

        await subject.SaveAsync(rules, "rules.json", false);

        subject.SerialisedData.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Save_ShouldThrow_GivenNullRules()
    {
        var subject = ArrangeUsingEmbeddedResources();
        await Should.ThrowAsync<ArgumentNullException>(async () => await subject.SaveAsync(null!, "rules.json", false));
    }

    [Fact]
    public async Task LoadAndSave_ShouldProduceSameJson()
    {
        var subject = ArrangeUsingEmbeddedResources();
        var rules = await subject.LoadAsync(TestDataConstants.DemoRulesFileName, false);

        await subject.SaveAsync(rules, "foo.bar", false);
        var serialisedData = JsonHelper.MinifyJson(subject.SerialisedData);
        var expectedData = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.DemoRulesFileName);
        expectedData = JsonHelper.MinifyJson(expectedData);

        serialisedData.ShouldBe(expectedData);
    }

    private JsonOnDiskMatchingRuleRepositoryTestHarness ArrangeUsingEmbeddedResources()
    {
        return new JsonOnDiskMatchingRuleRepositoryTestHarness(
            new XUnitLogger(this.output),
            new LocalDiskReaderWriterSelector([new EmbeddedResourceFileReaderWriter(), this.encryptedReaderWriter]),
            this.bucketRepo);
    }

    private JsonOnDiskMatchingRuleRepositoryTestHarness ArrangeUsingMocks()
    {
        return new JsonOnDiskMatchingRuleRepositoryTestHarness(new XUnitLogger(this.output), this.mockSelector, this.bucketRepo);
    }
}
