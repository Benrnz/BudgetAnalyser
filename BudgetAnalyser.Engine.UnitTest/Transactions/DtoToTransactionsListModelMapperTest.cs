using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.Transactions.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Transactions;

[TestClass]
public class DtoToTransactionsListModelMapperTest
{
    private TransactionsListModel Result { get; set; }
    private TransactionSetDto TestData => TransactionSetDtoTestData.TestData2();

    [TestMethod]
    public void ChangeHashShouldNotBeNull()
    {
        Assert.IsNotNull(Result.SignificantDataChangeHash());
    }

    [TestMethod]
    public void ShouldBeUnfiltered()
    {
        Assert.IsFalse(Result.Filtered);
    }

    [TestMethod]
    public void ShouldMapAllTransactions()
    {
        Assert.AreEqual(TestData.Transactions.Count(), Result.AllTransactions.Count());
    }

    [TestMethod]
    public void ShouldMapAllTransactionsAndHaveSameSum()
    {
        Assert.AreEqual(TestData.Transactions.Sum(t => t.Amount), Result.AllTransactions.Sum(t => t.Amount));
        Assert.AreEqual(TestData.Transactions.Sum(t => t.Date.DayNumber), Result.AllTransactions.Sum(t => t.Date.DayNumber));
    }

    [TestMethod]
    public void ShouldMapDurationInMonths()
    {
        Assert.AreEqual(2, Result.DurationInMonths);
    }

    [TestMethod]
    public void ShouldMapFileName()
    {
        Assert.AreEqual(TestData.StorageKey, Result.StorageKey);
    }

    [TestMethod]
    public void ShouldMapLastImport()
    {
        Assert.AreEqual(TestData.LastImport, Result.LastImport.ToUniversalTime());
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var subject = new MapperTransactionsListModelToDto2(
            new InMemoryAccountTypeRepository(),
            new BucketBucketRepoAlwaysFind(),
            new InMemoryTransactionTypeRepository(),
            new FakeLogger());
        Result = subject.ToModel(TestData);
    }

    [TestMethod]
    public void TransactionsShouldBeInAscendingOrder()
    {
        var previous = DateOnly.MinValue;
        foreach (var txn in Result.AllTransactions)
        {
            if (txn.Date < previous)
            {
                Assert.Fail();
            }

            previous = txn.Date;
        }
    }
}
