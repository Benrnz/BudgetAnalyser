using System.Reflection;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class BudgetItemTest
{
    [Fact]
    public void ShouldHaveAKnownNumberOfProperties()
    {
        var properties = typeof(BudgetItem).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
        properties.Count().ShouldBe(2);
    }

    [Fact]
    public void TwoBudgetItemsAreDifferentIfCodesAreDifferent()
    {
        var subject1 = CreateSubject1();
        var subject2 = CreateSubject2();

        subject1.ShouldNotBe(subject2);
        (subject1 != subject2).ShouldBeTrue();
        subject1.GetHashCode().ShouldNotBe(subject2.GetHashCode());
    }

    [Fact]
    public void TwoBudgetItemsAreDifferentIfCodesAreEqualButDifferentTypes()
    {
        var subject1 = CreateSubject1();
        var subject3 = CreateSubject3();

        subject1.ShouldNotBe(subject3);
        (subject1 != subject3).ShouldBeTrue();
        subject1.GetHashCode().ShouldNotBe(subject3.GetHashCode());
    }

    [Fact]
    public void TwoBudgetItemsAreTheSameIfCodesAreEqualAndSameType()
    {
        var subject1 = CreateSubject1();
        var subject2 = CreateSubject1();

        subject1.ShouldBe(subject2);
        (subject1 == subject2).ShouldBeTrue();
        subject1.GetHashCode().ShouldBe(subject2.GetHashCode());
    }

    [Fact]
    public void TwoReferencesToTheSameObjectAreEqual()
    {
        var subject1 = CreateSubject1();
        var subject2 = subject1;

        subject1.ShouldBe(subject2);
        (subject1 == subject2).ShouldBeTrue();
        subject1.GetHashCode().ShouldBe(subject2.GetHashCode());
    }

    private static BudgetBucket CreateBucket1()
    {
        return new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Foo bar");
    }

    private static BudgetBucket CreateBucket2()
    {
        return new SavedUpForExpenseBucket(TestDataConstants.PowerBucketCode, "Foo bar");
    }

    private static BudgetBucket CreateBucket3()
    {
        return new IncomeBudgetBucket(TestDataConstants.CarMtcBucketCode, "Foo bar");
    }

    private static BudgetItem CreateSubject1()
    {
        return new Expense { Amount = 0.01M, Bucket = CreateBucket1() };
    }

    private static BudgetItem CreateSubject2()
    {
        return new Expense { Amount = 0.01M, Bucket = CreateBucket2() };
    }

    private static BudgetItem CreateSubject3()
    {
        return new Income { Amount = 0.01M, Bucket = CreateBucket3() };
    }
}
