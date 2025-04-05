using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.XUnit.TestData;

public static class BudgetBucketTestData
{
    public static IEnumerable<BudgetBucket> BudgetModelTestData1Buckets
    {
        get
        {
            var budgetModel = BudgetModelTestData.CreateTestData1();
            return budgetModel.Expenses.Select(e => e.Bucket)
                .Union(budgetModel.Incomes.Select(i => i.Bucket));
        }
    }
}
