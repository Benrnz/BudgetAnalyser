using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Matching;

public class DataMatchingRuleToMatchingRuleMapperTest
{
    private readonly MatchingRuleDto testData;

    public DataMatchingRuleToMatchingRuleMapperTest()
    {
        this.testData = new MatchingRuleDto(
            Created: new DateTime(2014, 1, 2),
            Amount: 123.45M,
            BucketCode: "CARMTC",
            Description: "Testing Description",
            LastMatch: new DateTime(2014, 06, 22),
            MatchCount: 2,
            Reference1: "Testing Reference1",
            Reference2: "Testing Reference2",
            Reference3: "Testing Reference3",
            TransactionType: "Testing TransactionType",
            And: true);
    }

    [Fact]
    public void AmountShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.Amount.ShouldBe(this.testData.Amount);
    }

    [Fact]
    public void BucketCodeShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.BucketCode.ShouldBe(this.testData.BucketCode);
    }

    [Fact]
    public void CreatedDatesShouldBeMapped()
    {
        var result = ArrangeAndAct();
        ((DateTime?)result.Created).ShouldBe(this.testData.Created?.ToLocalTime());
    }

    [Fact]
    public void DescriptionShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.Description.ShouldBe(this.testData.Description);
    }

    [Fact]
    public void LastMatchShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.LastMatch.ShouldBe(this.testData.LastMatch?.ToLocalTime());
    }

    [Fact]
    public void MatchCountShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.MatchCount.ShouldBe(this.testData.MatchCount);
    }

    [Fact]
    public void NumberOfDataMatchingRulePropertiesShouldBe12()
    {
        var dataProperties = typeof(MatchingRule).CountProperties();
        dataProperties.ShouldBe(13);
    }

    [Fact]
    public void Reference1ShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.Reference1.ShouldBe(this.testData.Reference1);
    }

    [Fact]
    public void Reference2ShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.Reference2.ShouldBe(this.testData.Reference2);
    }

    [Fact]
    public void Reference3ShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.Reference3.ShouldBe(this.testData.Reference3);
    }

    [Fact]
    public void RuleIdShouldBeMapped()
    {
        var result = ArrangeAndAct();
        ((Guid?)result.RuleId).ShouldBe(this.testData.RuleId);
    }

    [Fact]
    public void TransactionTypeShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.TransactionType.ShouldBe(this.testData.TransactionType);
    }

    private MatchingRule ArrangeAndAct()
    {
        var subject = new MapperMatchingRuleToDto2(new BudgetBucketRepoAlwaysFind());
        return subject.ToModel(this.testData);
    }
}
