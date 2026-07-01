using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.Transactions.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class DtoToTransactionMapperTest
{
    private static readonly Guid TransactionId = new("7F921750-4467-4EA4-81E6-3EFD466341C6");
    private readonly Transaction result;

    public DtoToTransactionMapperTest()
    {
        var subject = new MapperTransactionToDto2(new InMemoryAccountTypeRepository(), new BudgetBucketRepoAlwaysFind(), new InMemoryTransactionTypeRepository());
        this.result = subject.ToModel(TestData);
    }

    private TransactionDto TestData => new(
        Id: TransactionId,
        Account: TransactionsListModelTestData.ChequeAccount.Name,
        Amount: 123.99M,
        BudgetBucketCode: TestDataConstants.PowerBucketCode,
        Date: new DateOnly(2014, 07, 31),
        Description: "The quick brown poo",
        Reference1: "Reference 1",
        Reference2: "REference 23",
        Reference3: "REference 33",
        TransactionType: "Credit Card Debit");

    [Fact]
    public void ShouldMapAccountType()
    {
        this.result.Account.Name.ShouldBe(TestData.Account);
    }

    [Fact]
    public void ShouldMapAmount()
    {
        this.result.Amount.ShouldBe(TestData.Amount);
    }

    [Fact]
    public void ShouldMapBucketCode()
    {
        this.result.BudgetBucket!.Code.ShouldBe(TestData.BudgetBucketCode);
    }

    [Fact]
    public void ShouldMapDate()
    {
        this.result.Date.ShouldBe(TestData.Date);
    }

    [Fact]
    public void ShouldMapDescription()
    {
        this.result.Description.ShouldBe(TestData.Description);
    }

    [Fact]
    public void ShouldMapId()
    {
        this.result.Id.ShouldBe(TransactionId);
    }

    [Fact]
    public void ShouldMapReference1()
    {
        this.result.Reference1.ShouldBe(TestData.Reference1);
    }

    [Fact]
    public void ShouldMapReference2()
    {
        this.result.Reference2.ShouldBe(TestData.Reference2);
    }

    [Fact]
    public void ShouldMapReference3()
    {
        this.result.Reference3.ShouldBe(TestData.Reference3);
    }

    [Fact]
    public void ShouldMapTransactionType()
    {
        this.result.TransactionType.Name.ShouldBe(TestData.TransactionType);
    }
}
