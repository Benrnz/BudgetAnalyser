using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Matching;

public class MatchingRuleTest
{
    private readonly IBudgetBucketRepository bucketRepo = new BudgetBucketRepoAlwaysFind();

    [Fact]
    public void AndMatchShouldFailWhenBucketIsInactive()
    {
        var subject = Arrange();
        subject.Description = "Testing Description";
        subject.Bucket.Active = false;

        var success = subject.Match(new Transaction { Description = "Testing Description" });
        success.ShouldBeFalse();
    }

    [Fact]
    public void AndMatchShouldMatchOnAmountAndDescription()
    {
        var subject = Arrange();
        subject.Description = "Testing Description";
        subject.Amount = 11.01M;

        var success = subject.Match(new Transaction { Description = "Testing Description", Amount = 11.01M });
        success.ShouldBeTrue();
    }

    [Fact]
    public void AndMatchShouldMatchOnDescription()
    {
        var subject = Arrange();
        subject.Description = "Testing Description";

        var success = subject.Match(new Transaction { Description = "Testing Description" });
        success.ShouldBeTrue();
    }

    [Fact]
    public void AndMatchShouldMatchOnReferences()
    {
        var subject = Arrange();
        subject.Reference1 = "Testing 1";
        subject.Reference2 = "Testing 2";
        subject.Reference3 = "Testing 3";

        var success = subject.Match(new Transaction { Reference1 = "Testing 1", Reference2 = "Testing 2", Reference3 = "Testing 3" });
        success.ShouldBeTrue();
    }

    [Fact]
    public void AndMatchShouldMatchOnTransacionType()
    {
        var subject = Arrange();
        subject.Description = "Testing Description";
        subject.TransactionType = "Foo";

        var success = subject.Match(new Transaction { Description = "Testing Description", TransactionType = new NamedTransaction("Foo") });
        success.ShouldBeTrue();
    }

    [Fact]
    public void AndMatchShouldNotMatchOnDescription1()
    {
        var subject = Arrange();
        subject.Description = "Testing Description";

        var success = subject.Match(new Transaction { Description = "xxxTesting Description" });
        success.ShouldBeFalse();
    }

    [Fact]
    public void AndMatchShouldNotMatchOnDescription2()
    {
        var subject = Arrange();
        subject.Description = "Testing Description";

        var success = subject.Match(new Transaction { Description = "Testing Descriptionxxx" });
        success.ShouldBeFalse();
    }

    [Fact]
    public void AndMatchShouldNotMatchOnDescription3()
    {
        var subject = Arrange();
        subject.Description = "Testing Description";
        subject.Reference1 = "Ref1";

        var success = subject.Match(new Transaction { Description = "Testing Description" });
        success.ShouldBeFalse();
    }

    [Fact]
    public void AnObjectIsNotEqualWithNull()
    {
        var subject = Arrange();
        subject.Equals(null).ShouldBeFalse();
        subject.ShouldNotBeNull();
    }

    [Fact]
    public void CtorShouldInitialiseCreated()
    {
        var subject = Arrange();
        subject.Created.ShouldNotBe(DateTime.MinValue);
    }

    [Fact]
    public void CtorShouldInitialiseId()
    {
        var subject = Arrange();
        subject.RuleId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void CtorShouldThrowIfNullBucketRepo()
    {
        Should.Throw<ArgumentNullException>(() => new MatchingRule(null!));
    }

    [Fact]
    public void MatchShouldSetLastMatchWhenMatchIsMade()
    {
        var subject = Arrange();
        subject.Description = "Testing Description";
        subject.LastMatch = DateTime.MinValue;
        subject.Match(new Transaction { Description = "Testing Description" });
        (subject.LastMatch > DateTime.MinValue).ShouldBeTrue();
    }

    [Fact]
    public void MatchShouldSetMatchCountWhenMatchIsMade()
    {
        var subject = Arrange();
        subject.Description = "Testing Description";
        subject.MatchCount = 1;
        subject.Match(new Transaction { Description = "Testing Description" });
        subject.MatchCount.ShouldBe(2);
    }

    [Fact]
    public void MatchShouldThrowWhenTransactionIsNull()
    {
        var subject = Arrange();
        Should.Throw<ArgumentNullException>(() => subject.Match(null!));
    }

    [Fact]
    public void OrMatchShouldMatchOnAmountAndDescription()
    {
        var subject = Arrange();
        subject.And = false;
        subject.Description = "Testing Description";
        subject.Amount = 11.01M;

        var success = subject.Match(new Transaction { Description = "Testing xxxx", Amount = 11.01M });
        success.ShouldBeTrue();
    }

    [Fact]
    public void OrMatchShouldMatchOnDescription3()
    {
        var subject = Arrange();
        subject.And = false;
        subject.Description = "Testing Description";
        subject.Reference1 = "Ref1";

        var success = subject.Match(new Transaction { Description = "Testing Description" });
        success.ShouldBeTrue();
    }

    [Fact]
    public void OrMatchShouldMatchOnReferences()
    {
        var subject = Arrange();
        subject.And = false;
        subject.Reference1 = "Testing 1";
        subject.Reference2 = "Testing 2";
        subject.Reference3 = "Testing 3";

        var success = subject.Match(new Transaction { Reference1 = "Testing 1", Reference3 = "Testing xxx" });
        success.ShouldBeTrue();
    }

    [Fact]
    public void OrMatchShouldMatchOnTransacionType()
    {
        var subject = Arrange();
        subject.And = false;
        subject.Description = "Testing Description";
        subject.TransactionType = "Foo";

        var success = subject.Match(new Transaction { Description = "Testing Description", TransactionType = new NamedTransaction("Foo") });
        success.ShouldBeTrue();
    }

    [Fact]
    public void OrMatchShouldNotMatchOnDescription1()
    {
        var subject = Arrange();
        subject.And = false;
        subject.Description = "Testing Description";

        var success = subject.Match(new Transaction { Description = "xxxTesting Description" });
        success.ShouldBeFalse();
    }

    [Fact]
    public void OrMatchShouldNotMatchOnDescription2()
    {
        var subject = Arrange();
        subject.And = false;
        subject.Description = "Testing Description";

        var success = subject.Match(new Transaction { Description = "Testing Descriptionxxx" });
        success.ShouldBeFalse();
    }

    [Fact]
    public void RulesWithDifferentIdAreConsideredNotEqual()
    {
        var subject = Arrange();
        var subject2 = Arrange();

        subject.Equals(subject2).ShouldBeFalse();
        (subject != subject2).ShouldBeTrue();
        (subject.GetHashCode() != subject2.GetHashCode()).ShouldBeTrue();
    }

    [Fact]
    public void RulesWithSameIdAreConsideredEqual()
    {
        var subject = Arrange();
        var subject2 = Arrange(subject.RuleId);
        subject.Equals(subject2).ShouldBeTrue();
        (subject == subject2).ShouldBeTrue();
        subject.GetHashCode().ShouldBe(subject2.GetHashCode());
    }

    [Fact]
    public void TwoObjectsReferringToSameAreEqual()
    {
        var subject = Arrange();
        var subject2 = subject;
        subject.Equals(subject2).ShouldBeTrue();
        (subject == subject2).ShouldBeTrue();
        subject.GetHashCode().ShouldBe(subject2.GetHashCode());
    }

    [Fact]
    public void TwoObjectsReferringToSameAreEqual2()
    {
        var subject = Arrange();
        object subject2 = subject;
        subject.Equals(subject2).ShouldBeTrue();
#pragma warning disable 252,253
        (subject == subject2).ShouldBeTrue();
#pragma warning restore 252,253
        subject.GetHashCode().ShouldBe(subject2.GetHashCode());
    }

    private MatchingRule Arrange(Guid? id = null)
    {
        return new MatchingRule(this.bucketRepo) { BucketCode = TransactionsListModelTestData.PowerBucket.Code, RuleId = id ?? Guid.NewGuid() };
    }
}
