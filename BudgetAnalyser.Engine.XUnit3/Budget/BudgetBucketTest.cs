using System.Reflection;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class BudgetBucketTest
{
    private const string NotSpecified = "NotSpecified";

    [Fact]
    public void BudgetBucketPropertiesShouldBeMapped()
    {
        var properties = typeof(BudgetBucket).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
        properties.Count().ShouldBe(3);
    }

    [Fact]
    public void Comparable_HairBucketIsLessThanPower()
    {
        var hairBucket = TransactionsListModelTestData.HairBucket;
        var powerBucket = TransactionsListModelTestData.PowerBucket;

        (hairBucket.CompareTo(powerBucket) < 0).ShouldBeTrue();
    }

    [Fact]
    public void Comparable_PhoneBucketIsEqualToAnotherInstance()
    {
        var phoneBucket = TransactionsListModelTestData.PhoneBucket;
        var phooneBucket2 = Arrange(TestDataConstants.PhoneBucketCode, "Foo");

        (phoneBucket.CompareTo(phooneBucket2) == 0).ShouldBeTrue();
    }

    [Fact]
    public void Comparable_PhoneBucketIsGreaterThanHair()
    {
        var hairBucket = TransactionsListModelTestData.HairBucket;
        var phoneBucket = TransactionsListModelTestData.PhoneBucket;

        (phoneBucket.CompareTo(hairBucket) > 0).ShouldBeTrue();
    }

    [Fact]
    public void CtorShouldAllocateUpperCaseCode()
    {
        var subject = Arrange("Foo", "Bar");
        subject.Code.ShouldBe("FOO");
        subject.Code.ShouldNotBe("Foo");
    }

    [Fact]
    public void CtorShouldThrowWhenCodeIsNull()
    {
        Should.Throw<ArgumentNullException>(() => Arrange(NotSpecified, "Something"));
    }

    [Fact]
    public void CtorShouldThrowWhenNameIsNull()
    {
        Should.Throw<ArgumentNullException>(() => Arrange("Something"));
    }

    [Fact]
    public void SettingCodeShouldConvertToUpperCase()
    {
        var subject = Arrange("Foo", "Bar");
        subject.Code = "White";
        subject.Code.ShouldNotBe("White");
        subject.Code.ShouldBe("WHITE");
    }

    [Fact]
    public void TwoBucketsAreDifferentIfCodesAreDifferent()
    {
        var subject1 = Arrange("Foo1", "Name");
        var subject2 = Arrange("Foo2", "Name");
        subject1.ShouldNotBe(subject2);
        (subject1 != subject2).ShouldBeTrue();
        subject1.GetHashCode().ShouldNotBe(subject2.GetHashCode());
    }

    [Fact]
    public void TwoBucketsAreTheSameIfCodesAreEqual()
    {
        var subject1 = Arrange("Foo", "Name");
        var subject2 = Arrange("Foo", "Name");
        subject1.ShouldBe(subject2);
        (subject1 == subject2).ShouldBeTrue();
        subject1.GetHashCode().ShouldBe(subject2.GetHashCode());
    }

    [Fact]
    public void TwoReferencesToDifferentObjectsAreNotEqual()
    {
        var subject1 = Arrange("Foo", "Name");
        var subject2 = Arrange("Ben", "Is Awesome");
        subject1.ShouldNotBe(subject2);
        (subject1 != subject2).ShouldBeTrue();
        subject1.GetHashCode().ShouldNotBe(subject2.GetHashCode());
    }

    [Fact]
    public void TwoReferencesToTheSameObjectAreEqual()
    {
        var subject1 = Arrange("Foo", "Name");
        var subject2 = subject1;
        subject1.ShouldBe(subject2);
        (subject1 == subject2).ShouldBeTrue();
        subject1.GetHashCode().ShouldBe(subject2.GetHashCode());
    }

    [Fact]
    public void ValidateShouldRetrunFalseGivenLongCode()
    {
        var subject = Arrange();
        subject.Code = "ABC345678";
        var result = subject.Validate(new StringBuilder());
        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidateShouldThrowGivenNullStringBuilder()
    {
        var subject = Arrange();
        Should.Throw<ArgumentNullException>(() => subject.Validate(null!));
    }

    [Fact]
    public void ValidateWillReturnFalseWhenCodeIsNull()
    {
        var subject = Arrange();
        subject.Description = "Foo bar";
        var builder = new StringBuilder();
        subject.Validate(builder).ShouldBeFalse();
        builder.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ValidateWillReturnFalseWhenCodeIsTooLong()
    {
        var subject = Arrange();
        subject.Description = "FooBarHo";
        var builder = new StringBuilder();
        subject.Validate(builder).ShouldBeFalse();
        builder.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ValidateWillReturnFalseWhenNameIsNull()
    {
        var subject = Arrange();
        subject.Code = "Foo";
        var builder = new StringBuilder();
        subject.Validate(builder).ShouldBeFalse();
        builder.Length.ShouldBeGreaterThan(0);
    }

    private static BudgetBucket Arrange(string code = NotSpecified, string name = NotSpecified)
    {
        if (code == NotSpecified && name == NotSpecified)
        {
            return new BudgetBucketTestHarness();
        }

        var code2 = code == NotSpecified ? null : code;
        var name2 = name == NotSpecified ? null : name;
        return new BudgetBucketTestHarness(code2, name2);
    }
}
