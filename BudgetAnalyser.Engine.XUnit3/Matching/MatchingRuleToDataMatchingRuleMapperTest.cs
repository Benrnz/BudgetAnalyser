using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Matching;

public class MatchingRuleToDataMatchingRuleMapperTest
{
    private readonly MatchingRule testData;

    public MatchingRuleToDataMatchingRuleMapperTest()
    {
        this.testData = new MatchingRule(new BudgetBucketRepoAlwaysFind())
        {
            Amount = 123.45M,
            BucketCode = "CARMTC",
            Description = "Testing Description",
            LastMatch = new DateTime(2014, 06, 22),
            MatchCount = 2,
            Reference1 = "Testing Reference1",
            Reference2 = "Testing Reference2",
            Reference3 = "Testing Reference3",
            TransactionType = "Testing TransactionType"
        };
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
        (result.Created?.ToLocalTime() == this.testData.Created).ShouldBeTrue();
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
        (result.LastMatch?.ToLocalTime() == this.testData.LastMatch).ShouldBeTrue();
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
        var dataProperties = typeof(MatchingRuleDto).CountProperties();
        dataProperties.ShouldBe(12);
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
        (result.RuleId == this.testData.RuleId).ShouldBeTrue();
    }

    [Fact]
    public void TransactionTypeShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.TransactionType.ShouldBe(this.testData.TransactionType);
    }

    private MatchingRuleDto ArrangeAndAct()
    {
        var subject = new MapperMatchingRuleToDto2(new BudgetBucketRepoAlwaysFind());
        return subject.ToDto(this.testData);
    }
}
