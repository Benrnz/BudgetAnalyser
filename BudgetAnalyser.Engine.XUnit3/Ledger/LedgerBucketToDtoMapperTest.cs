#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class LedgerBucketToDtoMapperTest
{
    public LedgerBucketToDtoMapperTest()
    {
        var bucketRepo = new BudgetBucketRepoAlwaysFind();
        var accountRepo = new InMemoryAccountTypeRepository();
        var subject = new MapperLedgerBucketToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo));
        Result = subject.ToDto(TestData);
    }

    private LedgerBucketDto Result { get; }

    private LedgerBucket TestData => LedgerBookTestData.RegoLedger;

    [Fact]
    public void ShouldMapBankAccount()
    {
        Result.StoredInAccount.ShouldBe(TransactionsListModelTestData.ChequeAccount.Name);
    }

    [Fact]
    public void ShouldMapBudgetBucketCode()
    {
        Result.BucketCode.ShouldBe(TestDataConstants.RegoBucketCode);
    }
}
