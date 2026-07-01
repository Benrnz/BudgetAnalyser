using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class BudgetModelTest
{
    private StringBuilder Logs { get; } = new();

    [Fact]
    public void AfterConstructionEffectiveDateIsValidDate()
    {
        var subject = new BudgetModel();

        subject.EffectiveFrom.ShouldNotBe(DateOnly.MinValue);
    }

    [Fact]
    public void AfterConstructionLastModifiedDateIsValidDate()
    {
        var subject = new BudgetModel();

        subject.LastModified.ShouldNotBe(DateTime.MinValue);
    }

    [Fact]
    public void AfterInitialiseExpensesShouldBeInDescendingOrder()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        EnsureDescendingOrder(subject.Expenses);
    }

    [Fact]
    public void AfterInitialiseIncomesShouldBeInDescendingOrder()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        EnsureDescendingOrder(subject.Incomes);
    }

    [Fact]
    public void AfterUpdateExpensesAreReplaced()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var expenses = new List<Expense>
        {
            new() { Amount = 4444, Bucket = new SpentPerPeriodExpenseBucket("Horse", "Shit") },
            new() { Amount = 9999, Bucket = new SavedUpForExpenseBucket("Foo", "Bar") }
        };

        subject.Update(subject.Incomes, expenses);

        subject.Expenses.Sum(e => e.Amount).ShouldBe(4444M + 9999M);
    }

    [Fact]
    public void AfterUpdateExpensesAreStillInDescendingOrder()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var expenses = new List<Expense>
        {
            new() { Amount = 4444, Bucket = new SpentPerPeriodExpenseBucket("Horse", "Shit") },
            new() { Amount = 9999, Bucket = new SavedUpForExpenseBucket("Foo", "Bar") }
        };

        subject.Update(subject.Incomes, expenses);

        EnsureDescendingOrder(subject.Expenses);
    }

    [Fact]
    public void AfterUpdateIncomesAreReplaced()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var incomes = new List<Income>
        {
            new() { Amount = 9999, Bucket = new IncomeBudgetBucket("Foo", "Bar") },
            new() { Amount = 4444, Bucket = new IncomeBudgetBucket("Horse", "Shit") }
        };

        subject.Update(incomes, subject.Expenses);

        subject.Incomes.Sum(i => i.Amount).ShouldBe(9999M + 4444M);
    }

    [Fact]
    public void AfterUpdateIncomesAreStillInDescendingOrder()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var incomes = new List<Income>
        {
            new() { Amount = 4444, Bucket = new IncomeBudgetBucket("Horse", "Shit") },
            new() { Amount = 9999, Bucket = new IncomeBudgetBucket("Foo", "Bar") }
        };

        subject.Update(incomes, subject.Expenses);

        EnsureDescendingOrder(subject.Incomes);
    }

    [Fact]
    public void AfterUpdateLastModifiedIsUpdated()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var lastUpdated = subject.LastModified;

        Thread.Sleep(10);
        subject.Update(subject.Incomes, subject.Expenses);

        subject.LastModified.ShouldNotBe(lastUpdated);
    }

    [Fact]
    public void CalculatedSurplusShouldBeExpectedValue()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        subject.Surplus.ShouldBe(1175M);
    }

    [Fact]
    public void ListsAreInitialised()
    {
        var subject = new BudgetModel();

        subject.Incomes.ShouldNotBeNull();
        subject.Expenses.ShouldNotBeNull();
    }

    [Fact]
    public void SurplusCannotBeUsedInTheExpenseList()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var myExpenses = subject.Expenses.ToList();
        myExpenses.Add(new Expense { Amount = 445M, Bucket = new SurplusBucket() });
        var myIncomes = subject.Incomes.ToList();

        Should.Throw<ValidationWarningException>(() => subject.Update(myIncomes, myExpenses));
    }

    private static void EnsureDescendingOrder(IEnumerable<BudgetItem> items)
    {
        var previousAmount = decimal.MaxValue;
        foreach (var item in items)
        {
            var current = item.Amount;
            current.ShouldBeLessThanOrEqualTo(previousAmount, "Expenses are not in descending order.");
            previousAmount = current;
        }
    }
}
