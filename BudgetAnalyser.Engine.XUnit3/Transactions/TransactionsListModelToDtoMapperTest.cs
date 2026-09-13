using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.Transactions.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class TransactionsListModelToDtoMapperTest
{
    private TransactionSetDto result = null!;

    public TransactionsListModelToDtoMapperTest()
    {
        Act(TestData);
    }

    private TransactionsListModel TestData => TransactionsListModelTestData.TestData2();

    [Fact]
    public void ShouldMapAllTransactions()
    {
        this.result.Transactions.Count().ShouldBe(TestData.AllTransactions.Count());
    }

    [Fact]
    public void ShouldMapAllTransactionsAndHaveSameSum()
    {
        this.result.Transactions.Sum(t => t.Amount).ShouldBe(TestData.AllTransactions.Sum(t => t.Amount));
        this.result.Transactions.Sum(t => t.Date.DayNumber).ShouldBe(TestData.AllTransactions.Sum(t => t.Date.DayNumber));
    }

    [Fact]
    public void ShouldMapAllTransactionsEvenWhenFiltered()
    {
        var testData = TestData;
        testData.Filter(new GlobalFilterCriteria { BeginDate = new DateOnly(2013, 07, 20), EndDate = new DateOnly(2013, 08, 19) });
        Act(testData);

        this.result.Transactions.Count().ShouldBe(TestData.AllTransactions.Count());
    }

    [Fact]
    public void ShouldMapFileName()
    {
        this.result.StorageKey.ShouldBe(TestData.StorageKey);
    }

    [Fact]
    public void ShouldMapLastImport()
    {
        this.result.LastImport.ToUniversalTime().ShouldBe(TestData.LastImport.ToUniversalTime());
    }

    private void Act(TransactionsListModel testData)
    {
        var subject = new MapperTransactionsListModelToDto2(
            new InMemoryAccountTypeRepository(),
            new BudgetBucketRepoAlwaysFind(),
            new InMemoryTransactionTypeRepository(),
            new FakeLogger());
        this.result = subject.ToDto(testData);
    }
}
