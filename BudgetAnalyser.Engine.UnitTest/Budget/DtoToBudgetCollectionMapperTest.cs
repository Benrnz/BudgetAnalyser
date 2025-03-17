using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Portable.Xaml;

namespace BudgetAnalyser.Engine.UnitTest.Budget;

[TestClass]
public class DtoToBudgetCollectionMapperTest
{
    [TestMethod]
    public void OutputBudgetModelTestData1()
    {
        var testData1 = new BudgetModelDto
        {
            EffectiveFrom = new DateOnly(2014, 4, 28),
            LastModified = new DateTime(2014, 5, 2),
            LastModifiedComment = "The quick brown fox jumped over the lazy dog.",
            Name = "Foo data budget",
            Incomes = new List<IncomeDto> { new() { Amount = 2300.23M, BudgetBucketCode = TestDataConstants.IncomeBucketCode } },
            Expenses = new List<ExpenseDto>
            {
                new() { Amount = 350.11M, BudgetBucketCode = TestDataConstants.PhoneBucketCode }, new() { Amount = 221.22M, BudgetBucketCode = TestDataConstants.PowerBucketCode }
            }
        };

        var testData2 = new BudgetModelDto
        {
            EffectiveFrom = new DateOnly(2012, 2, 29),
            LastModified = new DateTime(2013, 6, 6),
            LastModifiedComment = "Spatchcock.",
            Name = "Old data budget",
            Incomes = new List<IncomeDto> { new() { Amount = 2100.23M, BudgetBucketCode = TestDataConstants.IncomeBucketCode } },
            Expenses = new List<ExpenseDto>
            {
                new() { Amount = 310.11M, BudgetBucketCode = TestDataConstants.PhoneBucketCode }, new() { Amount = 111.22M, BudgetBucketCode = TestDataConstants.PowerBucketCode }
            }
        };

        var collection = new BudgetCollectionDto { Budgets = new List<BudgetModelDto> { testData1, testData2 }, StorageKey = "Foo.xml" };

        var serialised = XamlServices.Save(collection);
        Console.WriteLine(serialised);
    }
}
