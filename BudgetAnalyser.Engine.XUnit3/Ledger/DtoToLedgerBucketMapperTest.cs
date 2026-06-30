#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class DtoToLedgerBucketMapperTest
{
    public DtoToLedgerBucketMapperTest()
    {
        var bucketRepo = new BudgetBucketRepoAlwaysFind();
        var accountRepo = new InMemoryAccountTypeRepository();
        var subject = new MapperLedgerBucketToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo));
        Result = subject.ToModel(TestData);
    }

    private LedgerBucket Result { get; }

    private LedgerBucketDto TestData => new(TestDataConstants.RegoBucketCode, TestDataConstants.ChequeAccountName);

    [Fact]
    public void ShouldMapBankAccount()
    {
        Result.StoredInAccount.Name.ShouldBe(TransactionsListModelTestData.ChequeAccount.Name);
    }

    [Fact]
    public void ShouldMapBudgetBucketCode()
    {
        Result.BudgetBucket.Code.ShouldBe(TestDataConstants.RegoBucketCode);
    }
}
