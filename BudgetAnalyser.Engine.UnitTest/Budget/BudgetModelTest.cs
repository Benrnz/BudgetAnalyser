using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.UnitTest.TestData;

namespace BudgetAnalyser.Engine.UnitTest.Budget;

[TestClass]
public class BudgetModelTest
{
    public StringBuilder Logs { get; private set; }

    [TestMethod]
    public void AfterConstructionEffectiveDateIsValidDate()
    {
        var subject = new BudgetModel();

        Assert.AreNotEqual(DateOnly.MinValue, subject.EffectiveFrom);
    }

    [TestMethod]
    public void AfterConstructionLastModifiedDateIsValidDate()
    {
        var subject = new BudgetModel();

        Assert.AreNotEqual(DateTime.MinValue, subject.LastModified);
    }

    [TestMethod]
    public void AfterInitialiseExpensesShouldBeInDescendingOrder()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        EnsureDescendingOrder(subject.Expenses);
    }

    [TestMethod]
    public void AfterInitialiseIncomesShouldBeInDescendingOrder()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        EnsureDescendingOrder(subject.Incomes);
    }

    [TestMethod]
    public void AfterUpdateExpensesAreReplaced()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var expenses = new List<Expense>
        {
            new() { Amount = 4444, Bucket = new SpentPerPeriodExpenseBucket("Horse", "Shit") }, new() { Amount = 9999, Bucket = new SavedUpForExpenseBucket("Foo", "Bar") }
        };

        subject.Update(subject.Incomes, expenses);

        Assert.AreEqual(4444M + 9999M, subject.Expenses.Sum(e => e.Amount));
    }

    [TestMethod]
    public void AfterUpdateExpensesAreStillInDescendingOrder()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var expenses = new List<Expense>
        {
            new() { Amount = 4444, Bucket = new SpentPerPeriodExpenseBucket("Horse", "Shit") }, new() { Amount = 9999, Bucket = new SavedUpForExpenseBucket("Foo", "Bar") }
        };

        subject.Update(subject.Incomes, expenses);

        EnsureDescendingOrder(subject.Expenses);
    }

    [TestMethod]
    public void AfterUpdateIncomesAreReplaced()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var incomes = new List<Income> { new() { Amount = 9999, Bucket = new IncomeBudgetBucket("Foo", "Bar") }, new() { Amount = 4444, Bucket = new IncomeBudgetBucket("Horse", "Shit") } };

        subject.Update(incomes, subject.Expenses);

        Assert.AreEqual(9999M + 4444M, subject.Incomes.Sum(i => i.Amount));
    }

    [TestMethod]
    public void AfterUpdateIncomesAreStillInDescendingOrder()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var incomes = new List<Income> { new() { Amount = 4444, Bucket = new IncomeBudgetBucket("Horse", "Shit") }, new() { Amount = 9999, Bucket = new IncomeBudgetBucket("Foo", "Bar") } };

        subject.Update(incomes, subject.Expenses);

        EnsureDescendingOrder(subject.Incomes);
    }

    [TestMethod]
    public void AfterUpdateLastModifiedIsUpdated()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var lastUpdated = subject.LastModified;

        Thread.Sleep(10);
        subject.Update(subject.Incomes, subject.Expenses);

        Assert.AreNotEqual(lastUpdated, subject.LastModified);
    }

    [TestMethod]
    public void CalculatedSurplusShouldBeExpectedValue()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        Assert.AreEqual(1175M, subject.Surplus);
    }

    [TestMethod]
    public void ListsAreInitialised()
    {
        var subject = new BudgetModel();

        Assert.IsNotNull(subject.Incomes);
        Assert.IsNotNull(subject.Expenses);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationWarningException))]
    public void SurplusCannotBeUsedInTheExpenseList()
    {
        var subject = BudgetModelTestData.CreateTestData1();

        var myExpenses = subject.Expenses.ToList();
        myExpenses.Add(new Expense { Amount = 445M, Bucket = new SurplusBucket() });
        var myIncomes = subject.Incomes.ToList();

        subject.Update(myIncomes, myExpenses);

        Assert.Fail();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        Logs = new StringBuilder();
    }

    private static void EnsureDescendingOrder(IEnumerable<BudgetItem> items)
    {
        var previousAmount = decimal.MaxValue;
        foreach (var item in items)
        {
            var current = item.Amount;
            if (current > previousAmount)
            {
                Assert.Fail("Expenses are not in descending order.");
            }

            previousAmount = current;
        }
    }
}
