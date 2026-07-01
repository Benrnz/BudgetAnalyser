using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class IncomeTest
{
    private StringBuilder Logs { get; } = new();

    [Fact]
    public void BucketMustHaveADescription()
    {
        var subject = CreateSubject();
        subject.Bucket!.Description = null!;

        var result = subject.Validate(Logs);

        result.ShouldBeFalse();
        Logs.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void MaxDecimalIsValidIncome()
    {
        var subject = CreateSubject();
        subject.Amount = decimal.MaxValue;

        var result = subject.Validate(Logs);

        result.ShouldBeTrue();
        Logs.Length.ShouldBe(0);
    }

    [Fact]
    public void MustBeAnIncomeBucket()
    {
        var subject = CreateSubject();
        subject.Bucket = new SavedUpForExpenseBucket("Foo", "Bar");

        var result = subject.Validate(Logs);

        result.ShouldBeFalse();
        Logs.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void NegativeAmountIsNotValid()
    {
        var subject = CreateSubject();
        subject.Amount = -5;

        var result = subject.Validate(Logs);

        result.ShouldBeFalse();
        Logs.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void NegativeAmountIsNotValidEvenWhenInActive()
    {
        var subject = CreateSubject();
        subject.Amount = -5;
        subject.Bucket!.Active = false;

        var result = subject.Validate(Logs);

        result.ShouldBeFalse();
        Logs.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void OneCentIsValidIncome()
    {
        var subject = CreateSubject();

        var result = subject.Validate(Logs);

        result.ShouldBeTrue();
        Logs.Length.ShouldBe(0);
    }

    [Fact]
    public void ZeroAmountIsValid()
    {
        var subject = CreateSubject();
        subject.Amount = 0;

        var result = subject.Validate(Logs);

        result.ShouldBeTrue();
    }

    private static Income CreateSubject()
    {
        return new Income { Amount = 0.01M, Bucket = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, "Foo Bar") };
    }
}
