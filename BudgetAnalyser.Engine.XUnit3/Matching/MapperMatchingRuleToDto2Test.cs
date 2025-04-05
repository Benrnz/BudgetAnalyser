using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Matching;

public class MapperMatchingRuleToDto2Test
{
    private readonly IBudgetBucketRepository bucketRepo = new BudgetBucketRepoAlwaysFind();

    private readonly MatchingRule matchingRuleTestData;
    private readonly SingleUseMatchingRule singleUseRuleTestData;
    private readonly MapperMatchingRuleToDto2 subject;

    public MapperMatchingRuleToDto2Test()
    {
        this.subject = new MapperMatchingRuleToDto2(this.bucketRepo);
        this.matchingRuleTestData = new MatchingRule(this.bucketRepo)
        {
            Amount = 12.34M,
            And = true,
            BucketCode = SurplusBucket.SurplusCode,
            Description = "McDonalds",
            Created = new DateTime(2025, 1, 18),
            LastMatch = new DateTime(2025, 1, 1),
            MatchCount = 1,
            Reference1 = "Pakuranga",
            Reference2 = "Ref2",
            Reference3 = "Ref3",
            TransactionType = "TranType"
        };

        this.singleUseRuleTestData = new SingleUseMatchingRule(this.bucketRepo)
        {
            Amount = 12.34M,
            And = true,
            BucketCode = SurplusBucket.SurplusCode,
            Description = "McDonalds",
            Created = new DateTime(2025, 1, 18),
            LastMatch = new DateTime(2025, 1, 1),
            MatchCount = 1,
            Reference1 = "Pakuranga",
            Reference2 = "Ref2",
            Reference3 = "Ref3",
            TransactionType = "TranType"
        };
    }

    [Fact]
    public void ToDto_ShouldReturnMatchingRuleDto_GivenValidData()
    {
        var dto = this.subject.ToDto(this.matchingRuleTestData);

        dto.ShouldBeOfType<MatchingRuleDto>();
        dto.And.ShouldBeTrue();
        dto.Amount.ShouldBe(12.34M);
        dto.BucketCode.ShouldBe(SurplusBucket.SurplusCode);
        dto.Description.ShouldBe("McDonalds");
        dto.Created?.ToLocalTime().ShouldBe(new DateTime(2025, 1, 18));
        dto.LastMatch?.ToLocalTime().ShouldBe(new DateTime(2025, 1, 1));
        dto.MatchCount.ShouldBe(1);
        dto.Reference1.ShouldBe("Pakuranga");
        dto.Reference2.ShouldBe("Ref2");
        dto.Reference3.ShouldBe("Ref3");
        dto.TransactionType.ShouldBe("TranType");
    }

    [Fact]
    public void ToDto_ShouldReturnSingleUseRuleDto_GivenParentMatchingRule()
    {
        var dto = this.subject.ToDto(this.singleUseRuleTestData);

        dto.ShouldBeOfType<SingleUseMatchingRuleDto>();
    }

    [Fact]
    public void ToDto_ShouldReturnSingleUseRuleDto_GivenValidData()
    {
        var dto = this.subject.ToDto(this.singleUseRuleTestData);

        dto.And.ShouldBeTrue();
        dto.Amount.ShouldBe(12.34M);
        dto.BucketCode.ShouldBe(SurplusBucket.SurplusCode);
        dto.Description.ShouldBe("McDonalds");
        dto.Created?.ToLocalTime().ShouldBe(new DateTime(2025, 1, 18));
        dto.LastMatch?.ToLocalTime().ShouldBe(new DateTime(2025, 1, 1));
        dto.MatchCount.ShouldBe(1);
        dto.Reference1.ShouldBe("Pakuranga");
        dto.Reference2.ShouldBe("Ref2");
        dto.Reference3.ShouldBe("Ref3");
        dto.TransactionType.ShouldBe("TranType");
    }

    [Fact]
    public void ToModel_ShouldReturnMatchingRule_GivenValidData()
    {
        var dto = this.subject.ToDto(this.matchingRuleTestData);
        var model2 = this.subject.ToModel(dto);
        model2.ShouldBeEquivalentTo(this.matchingRuleTestData);
    }

    [Fact]
    public void ToModel_ShouldReturnSingleUseRule_GivenValidData()
    {
        var dto = this.subject.ToDto(this.singleUseRuleTestData);
        var model2 = this.subject.ToModel(dto);
        model2.ShouldBeEquivalentTo(this.singleUseRuleTestData);
    }
}
