using System.Reflection;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class BudgetCollectionTest
{
    [Fact]
    public void ForDate_1_1_2013_ShouldReturnBudget1()
    {
        var subject = Arrange();

        var result = subject.ForDate(new DateOnly(2013, 1, 1));

        result.ShouldBeSameAs(subject.First(b => b.Name == TestDataConstants.Budget1Name));
    }

    [Fact]
    public void ForDate_1_1_2014_ShouldReturnBudget1()
    {
        var subject = Arrange();

        var result = subject.ForDate(new DateOnly(2014, 1, 1));

        result.ShouldBeSameAs(subject.First(b => b.Name == TestDataConstants.Budget1Name));
    }

    [Fact]
    public void ForDate_25_1_2014_ShouldReturnBudget2()
    {
        var subject = Arrange();

        var result = subject.ForDate(new DateOnly(2014, 1, 25));

        result.ShouldBeSameAs(subject.First(b => b.Name == TestDataConstants.Budget2Name));
    }

    [Fact]
    public void ForDate_WithEarlierDateThanFirstBudget_ShouldReturnNull()
    {
        var subject = Arrange();

        var result = subject.ForDate(DateOnly.MinValue);

        result.ShouldBeNull();
    }

    [Fact]
    public void ForDates_1_1_2013_to_20_1_2014_ShouldReturnBudget1And2()
    {
        var subject = Arrange();
        var result = subject.ForDates(new DateOnly(2013, 1, 1), new DateOnly(2014, 1, 20));
        result.Count().ShouldBe(2);
    }

    [Fact]
    public void ForDates_WithDatesOutsideBudgetCollection_ShouldThrow()
    {
        var subject = Arrange();

        Should.Throw<BudgetException>(() => subject.ForDates(DateOnly.MinValue, DateOnly.MaxValue));
    }

    [Fact]
    public void OutputBudgetCollection()
    {
        var subject = Arrange();
        foreach (var budget in subject)
        {
            Console.WriteLine("Budget: '{0}' EffectiveFrom: {1:d}", budget.Name, budget.EffectiveFrom);
        }
    }

    [Fact]
    public void ShouldHaveAKnownNumberOfProperties()
    {
        var properties = typeof(BudgetCollection).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
        properties.Count().ShouldBe(1);
    }

    [Fact]
    public void ValidateShouldFixGivenBudgetsWithDuplicateEffectiveDates()
    {
        var subject = Arrange();
        subject.Add(new BudgetModelFake { EffectiveFrom = subject.First().EffectiveFrom, Name = Guid.NewGuid().ToString() });
        subject.Validate(new StringBuilder());

        (subject.GroupBy(b => b.EffectiveFrom).Sum(group => group.Count()) == 3).ShouldBeTrue();
    }

    [Fact]
    public void ValidateShouldReturnFalseGivenOneBadBudget()
    {
        var subject = Arrange();
        subject.Add(new BudgetModelFake
        {
            EffectiveFrom = DateOnlyExt.Today(),
            Name = "Foo123",
            InitialiseOverride = () => { },
            ValidateOverride = msg => false
        });

        var result = subject.Validate(new StringBuilder());

        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidateShouldReturnTrueGivenBudgetsWithDuplicateEffectiveDates()
    {
        var subject = Arrange();
        subject.Add(new BudgetModelFake { EffectiveFrom = subject.First().EffectiveFrom, Name = Guid.NewGuid().ToString() });

        subject.Validate(new StringBuilder()).ShouldBeTrue();
    }

    [Fact]
    public void ValidateShouldReturnTrueGivenGoodBudgets()
    {
        var subject = Arrange();
        var result = subject.Validate(new StringBuilder());
        result.ShouldBeTrue();
    }

    private static BudgetCollection Arrange()
    {
        return new BudgetCollection(BudgetModelTestData.CreateTestData2(), BudgetModelTestData.CreateTestData1());
    }
}
