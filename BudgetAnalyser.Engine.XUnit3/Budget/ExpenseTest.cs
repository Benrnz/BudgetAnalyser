using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class ExpenseTest
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
    public void Ctor_ShouldAllowNullBucket()
    {
        var subject = new Expense();
        subject.Bucket.ShouldBeNull();
        subject.ShouldNotBeNull();
        subject.BucketCode.ShouldBe("[Blank / Invalid]");
    }

    [Fact]
    public void MaxDeciamlIsValidAmount()
    {
        var subject = CreateSubject();
        subject.Amount = decimal.MaxValue;

        var result = subject.Validate(Logs);

        result.ShouldBeTrue();
        Logs.Length.ShouldBe(0);
    }

    [Fact]
    public void MustBeAnExpenseBucket()
    {
        var subject = CreateSubject();
        subject.Bucket = new IncomeBudgetBucket("Foo", "Bar");

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
    public void NegativeAmountIsNotValidEvenWhenInactive()
    {
        var subject = CreateSubject();
        subject.Amount = -5;
        subject.Bucket!.Active = false;

        var result = subject.Validate(Logs);

        result.ShouldBeFalse();
        Logs.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void OneCentIsValidAmount()
    {
        var subject = CreateSubject();

        var result = subject.Validate(Logs);

        result.ShouldBeTrue();
        Logs.Length.ShouldBe(0);
    }

    [Fact]
    public void ZeroAmountIsNotValidWhenActive()
    {
        var subject = CreateSubject();
        subject.Amount = 0;
        subject.Bucket!.Active = true;

        var result = subject.Validate(Logs);

        result.ShouldBeFalse();
        Logs.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ZeroAmountIsValidWhenInActive()
    {
        var subject = CreateSubject();
        subject.Amount = 0;
        subject.Bucket!.Active = false;

        var result = subject.Validate(Logs);

        result.ShouldBeTrue();
    }

    private Expense CreateSubject()
    {
        return new Expense { Amount = 0.01M, Bucket = new SpentPerPeriodExpenseBucket(TestDataConstants.CarMtcBucketCode, "Foo Bar") };
    }
}
