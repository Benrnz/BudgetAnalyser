using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.Transactions.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class DtoToTransactionsListModelMapperTest
{
    private readonly TransactionsListModel result;

    public DtoToTransactionsListModelMapperTest()
    {
        var subject = new MapperTransactionsListModelToDto2(
            new InMemoryAccountTypeRepository(),
            new BudgetBucketRepoAlwaysFind(),
            new InMemoryTransactionTypeRepository(),
            new FakeLogger());
        this.result = subject.ToModel(TestData);
    }

    private TransactionSetDto TestData => TransactionSetDtoTestData.TestData2();

    [Fact]
    public void ChangeHashShouldNotBeNull()
    {
        this.result.SignificantDataChangeHash().ShouldNotBe(0L);
    }

    [Fact]
    public void ShouldBeUnfiltered()
    {
        this.result.Filtered.ShouldBeFalse();
    }

    [Fact]
    public void ShouldMapAllTransactions()
    {
        this.result.AllTransactions.Count().ShouldBe(TestData.Transactions.Count());
    }

    [Fact]
    public void ShouldMapAllTransactionsAndHaveSameSum()
    {
        this.result.AllTransactions.Sum(t => t.Amount).ShouldBe(TestData.Transactions.Sum(t => t.Amount));
        this.result.AllTransactions.Sum(t => t.Date.DayNumber).ShouldBe(TestData.Transactions.Sum(t => t.Date.DayNumber));
    }

    [Fact]
    public void ShouldMapDurationInMonths()
    {
        this.result.DurationInMonths.ShouldBe(2);
    }

    [Fact]
    public void ShouldMapFileName()
    {
        this.result.StorageKey.ShouldBe(TestData.StorageKey);
    }

    [Fact]
    public void ShouldMapLastImport()
    {
        this.result.LastImport.ToUniversalTime().ShouldBe(TestData.LastImport);
    }

    [Fact]
    public void TransactionsShouldBeInAscendingOrder()
    {
        var previous = DateOnly.MinValue;
        foreach (var txn in this.result.AllTransactions)
        {
            txn.Date.ShouldBeGreaterThanOrEqualTo(previous);
            previous = txn.Date;
        }
    }
}
