using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.UnitTest.TestData;

namespace BudgetAnalyser.Engine.UnitTest.Budget;

[TestClass]
public class BudgetCurrencyContextTest
{
    [TestMethod]
    public void Budget1ShouldBeArchivedAfterConstruction()
    {
        var subject = CreateSubject1();

        Assert.IsFalse(subject.BudgetActive);
        Assert.IsTrue(subject.BudgetArchived);
        Assert.IsFalse(subject.BudgetInFuture);
    }

    [TestMethod]
    public void Budget1ShouldBeEffectiveUntilBudget2EffectiveDate()
    {
        var subject = CreateSubject1();

        Assert.IsTrue(subject.BudgetArchived);
        Assert.AreEqual(new DateOnly(2014, 01, 20), subject.EffectiveUntil);
    }

    [TestMethod]
    public void Budget2ShouldBeCurrentAfterConstruction()
    {
        var subject = CreateSubject2();

        Assert.IsTrue(subject.BudgetActive);
        Assert.IsFalse(subject.BudgetArchived);
        Assert.IsFalse(subject.BudgetInFuture);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CtorShouldThrowIfCollectionIsNull()
    {
        new BudgetCurrencyContext(null, BudgetModelTestData.CreateTestData1());
        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void CtorShouldThrowIfCurrentBudgetIsNotInCollection()
    {
        new BudgetCurrencyContext(BudgetModelTestData.CreateCollectionWith1And2(), new BudgetModel());
        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CtorShouldThrowIfModelIsNull()
    {
        new BudgetCurrencyContext(BudgetModelTestData.CreateCollectionWith1And2(), null);
        Assert.Fail();
    }

    [TestMethod]
    public void ModelShouldNotBeNullAfterConstruction()
    {
        var subject = CreateSubject1();

        Assert.IsNotNull(subject.Model);
    }

    [TestMethod]
    public void ShouldIndicateFutureBudgetWhenOneIsGiven()
    {
        var budget3 = new BudgetModel { EffectiveFrom = new DateOnly(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month, 28) };
        var subject = new BudgetCurrencyContext(
            new BudgetCollection(BudgetModelTestData.CreateTestData1(), BudgetModelTestData.CreateTestData2(), budget3),
            budget3
        );

        Assert.IsFalse(subject.BudgetActive);
        Assert.IsFalse(subject.BudgetArchived);
        Assert.IsTrue(subject.BudgetInFuture);
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
