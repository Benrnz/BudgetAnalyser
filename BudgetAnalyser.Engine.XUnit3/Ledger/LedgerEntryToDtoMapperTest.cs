#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class LedgerEntryToDtoMapperTest
{
    public LedgerEntryToDtoMapperTest()
    {
        var accountTypeRepo = new InMemoryAccountTypeRepository();
        var subject = new MapperLedgerEntryToDto2(new LedgerBucketFactory(new BudgetBucketRepoAlwaysFind(), accountTypeRepo), new LedgerTransactionFactory());

        Result = subject.ToDto(TestData);
    }

    private LedgerEntryDto Result { get; }
    private LedgerEntry TestData => LedgerBookTestData.TestData1().Reconciliations.First().Entries.First();

    [Fact]
    public void ShouldMapBalance()
    {
        Result.Balance.ShouldBe(120M);
    }

    [Fact]
    public void ShouldMapBucketCode()
    {
        Result.BucketCode.ShouldBe(TestDataConstants.HairBucketCode);
    }

    [Fact]
    public void ShouldMapCorrectNumberOfTransactions()
    {
        Result.Transactions.Count().ShouldBe(1);
    }
}
