using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Matching;

public class DtoToMatchingRuleMapperTest
{
    private static readonly Guid Id = new("901EC4BB-B8B5-43CD-A8C9-15121048CBA4");
    private readonly MatchingRule result;

    public DtoToMatchingRuleMapperTest()
    {
        var subject = new MapperMatchingRuleToDto2(new BudgetBucketRepoAlwaysFind());
        this.result = subject.ToModel(TestData);
    }

    private MatchingRuleDto TestData => new(
            123.45M,
            BucketCode: TestDataConstants.PhoneBucketCode,
            Created: new DateTime(2014, 07, 04),
            Description: "The quick brown fox",
            LastMatch: new DateTime(2014, 07, 29),
            MatchCount: 2,
            Reference1: "jumped",
            Reference2: "over",
            Reference3: "the lazy",
            TransactionType: "dog.",
            And: true)
    { RuleId = Id };

    [Fact]
    public void ShouldMapAmount()
    {
        this.result.Amount.ShouldBe(TestData.Amount);
    }

    [Fact]
    public void ShouldMapBucket()
    {
        this.result.Bucket.Code.ShouldBe(TestData.BucketCode);
    }

    [Fact]
    public void ShouldMapBucketCode()
    {
        this.result.BucketCode.ShouldBe(TestData.BucketCode);
    }

    [Fact]
    public void ShouldMapCreated()
    {
        ((DateTime?)this.result.Created).ShouldBe(TestData.Created?.ToLocalTime());
    }

    [Fact]
    public void ShouldMapDescription()
    {
        this.result.Description.ShouldBe(TestData.Description);
    }

    [Fact]
    public void ShouldMapId()
    {
        this.result.RuleId.ShouldBe(Id);
    }

    [Fact]
    public void ShouldMapLastMatch()
    {
        this.result.LastMatch.ShouldBe(TestData.LastMatch?.ToLocalTime());
    }

    [Fact]
    public void ShouldMapMatchCount()
    {
        this.result.MatchCount.ShouldBe(TestData.MatchCount);
    }

    [Fact]
    public void ShouldMapReference1()
    {
        this.result.Reference1.ShouldBe(TestData.Reference1);
    }

    [Fact]
    public void ShouldMapReference2()
    {
        this.result.Reference2.ShouldBe(TestData.Reference2);
    }

    [Fact]
    public void ShouldMapReference3()
    {
        this.result.Reference3.ShouldBe(TestData.Reference3);
    }

    [Fact]
    public void ShouldMapTransactionType()
    {
        this.result.TransactionType.ShouldBe(TestData.TransactionType);
    }
}
