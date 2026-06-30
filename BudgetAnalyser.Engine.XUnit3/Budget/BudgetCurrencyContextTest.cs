using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class BudgetCurrencyContextTest
{
    [Fact]
    public void Budget1ShouldBeArchivedAfterConstruction()
    {
        var subject = CreateSubject1();

        subject.BudgetActive.ShouldBeFalse();
        subject.BudgetArchived.ShouldBeTrue();
        subject.BudgetInFuture.ShouldBeFalse();
    }

    [Fact]
    public void Budget1ShouldBeEffectiveUntilBudget2EffectiveDate()
    {
        var subject = CreateSubject1();

        subject.BudgetArchived.ShouldBeTrue();
        subject.EffectiveUntil.ShouldBe(new DateOnly(2014, 01, 20));
    }

    [Fact]
    public void Budget2ShouldBeCurrentAfterConstruction()
    {
        var subject = CreateSubject2();

        subject.BudgetActive.ShouldBeTrue();
        subject.BudgetArchived.ShouldBeFalse();
        subject.BudgetInFuture.ShouldBeFalse();
    }

    [Fact]
    public void CtorShouldThrowIfCollectionIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new BudgetCurrencyContext(null!, BudgetModelTestData.CreateTestData1()));
    }

    [Fact]
    public void CtorShouldThrowIfCurrentBudgetIsNotInCollection()
    {
        Should.Throw<KeyNotFoundException>(() => new BudgetCurrencyContext(BudgetModelTestData.CreateCollectionWith1And2(), new BudgetModel()));
    }

    [Fact]
    public void CtorShouldThrowIfModelIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new BudgetCurrencyContext(BudgetModelTestData.CreateCollectionWith1And2(), null!));
    }

    [Fact]
    public void ModelShouldNotBeNullAfterConstruction()
    {
        var subject = CreateSubject1();

        subject.Model.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldIndicateFutureBudgetWhenOneIsGiven()
    {
        var budget3 = new BudgetModel { EffectiveFrom = new DateOnly(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month, 28) };
        var subject = new BudgetCurrencyContext(
            new BudgetCollection(BudgetModelTestData.CreateTestData1(), BudgetModelTestData.CreateTestData2(), budget3),
            budget3
        );

        subject.BudgetActive.ShouldBeFalse();
        subject.BudgetArchived.ShouldBeFalse();
        subject.BudgetInFuture.ShouldBeTrue();
    }

    private static BudgetCurrencyContext CreateSubject1()
    {
        var budget1 = BudgetModelTestData.CreateTestData1();
        var subject = new BudgetCurrencyContext(new BudgetCollection(budget1, BudgetModelTestData.CreateTestData2()), budget1);
        return subject;
    }

    private static BudgetCurrencyContext CreateSubject2()
    {
        var budget2 = BudgetModelTestData.CreateTestData2();
        var subject = new BudgetCurrencyContext(new BudgetCollection(BudgetModelTestData.CreateTestData1(), budget2), budget2);
        return subject;
    }
}
