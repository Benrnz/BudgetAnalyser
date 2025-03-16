using System.Reflection;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Budget;

[TestClass]
public class BudgetCollectionTest
{
    [TestMethod]
    public void ForDate_1_1_2013_ShouldReturnBudget1()
    {
        var subject = Arrange();

        var result = subject.ForDate(new DateOnly(2013, 1, 1));

        Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget1Name), result);
    }

    [TestMethod]
    public void ForDate_1_1_2014_ShouldReturnBudget1()
    {
        var subject = Arrange();

        var result = subject.ForDate(new DateOnly(2014, 1, 1));

        Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget1Name), result);
    }

    [TestMethod]
    public void ForDate_25_1_2014_ShouldReturnBudget2()
    {
        var subject = Arrange();

        var result = subject.ForDate(new DateOnly(2014, 1, 25));

        Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget2Name), result);
    }

    [TestMethod]
    public void ForDate_WithEarlierDateThanFirstBudget_ShouldReturnNull()
    {
        var subject = Arrange();

        var result = subject.ForDate(DateOnly.MinValue);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ForDates_1_1_2013_to_20_1_2014_ShouldReturnBudget1And2()
    {
        var subject = Arrange();
        var result = subject.ForDates(new DateOnly(2013, 1, 1), new DateOnly(2014, 1, 20));
        Assert.AreEqual(2, result.Count());
    }

    [TestMethod]
    [ExpectedException(typeof(BudgetException))]
    public void ForDates_WithDatesOutsideBudgetCollection_ShouldThrow()
    {
        var subject = Arrange();

        subject.ForDates(DateOnly.MinValue, DateOnly.MaxValue);

        Assert.Fail();
    }

    [TestMethod]
    public void OutputBudgetCollection()
    {
        var subject = Arrange();
        foreach (var budget in subject)
        {
            Console.WriteLine("Budget: '{0}' EffectiveFrom: {1:d}", budget.Name, budget.EffectiveFrom);
        }
    }

    [TestMethod]
    public void ShouldHaveAKnownNumberOfProperties()
    {
        // If this test breaks consider putting the new property into the Mappers and DTO's before updating the count.
        var properties = typeof(BudgetCollection).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
        Assert.AreEqual(1, properties.Count());
    }

    [TestMethod]
    public void ValidateShouldFixGivenBudgetsWithDuplicateEffectiveDates()
    {
        var subject = Arrange();
        subject.Add(
            new BudgetModelFake { EffectiveFrom = subject.First().EffectiveFrom, Name = Guid.NewGuid().ToString() });
        subject.Validate(new StringBuilder());

        Assert.IsTrue(subject.GroupBy(b => b.EffectiveFrom).Sum(group => group.Count()) == 3);
    }

    [TestMethod]
    public void ValidateShouldReturnFalseGivenOneBadBudget()
    {
        var subject = Arrange();
        subject.Add(
            new BudgetModelFake { EffectiveFrom = DateOnlyExt.Today(), Name = "Foo123", InitialiseOverride = () => { }, ValidateOverride = msg => false });

        var result = subject.Validate(new StringBuilder());

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ValidateShouldReturnTrueGivenBudgetsWithDuplicateEffectiveDates()
    {
        var subject = Arrange();
        subject.Add(
            new BudgetModelFake { EffectiveFrom = subject.First().EffectiveFrom, Name = Guid.NewGuid().ToString() });

        Assert.IsTrue(subject.Validate(new StringBuilder()));
    }

    [TestMethod]
    public void ValidateShouldReturnTrueGivenGoodBudgets()
    {
        var subject = Arrange();
        var result = subject.Validate(new StringBuilder());
        Assert.IsTrue(result);
    }

    private BudgetCollection Arrange()
    {
        return new BudgetCollection(BudgetModelTestData.CreateTestData2(), BudgetModelTestData.CreateTestData1());
    }
}
