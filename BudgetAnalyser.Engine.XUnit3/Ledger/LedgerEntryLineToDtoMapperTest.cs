#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class LedgerEntryLineToDtoMapperTest
{
    public LedgerEntryLineToDtoMapperTest()
    {
        var accountRepo = new InMemoryAccountTypeRepository();
        var subject = new MapperLedgerEntryLineToDto2(accountRepo, new LedgerBucketFactory(new BudgetBucketRepoAlwaysFind(), accountRepo), new LedgerTransactionFactory());
        Result = subject.ToDto(TestData);
    }

    private LedgerEntryLineDto Result { get; }

    private LedgerEntryLine TestData
    {
        get
        {
            var book = LedgerBookTestData.TestData4();
            return book.Reconciliations.First();
        }
    }

    [Fact]
    public void ShouldMapBankBalance()
    {
        Result.BankBalance.ShouldBe(2950M);
    }

    [Fact]
    public void ShouldMapBankBalanceAdjustments()
    {
        Result.BankBalanceAdjustments.Count().ShouldBe(1);
    }

    [Fact]
    public void ShouldMapBankBalances()
    {
        Result.BankBalances.Count().ShouldBe(2);
    }

    [Fact]
    public void ShouldMapDate()
    {
        Result.Date.ShouldBe(new DateOnly(2013, 8, 15));
    }

    [Fact]
    public void ShouldMapEntries()
    {
        Result.Entries.Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldMapRemarks()
    {
        Result.Remarks.ShouldBe("The quick brown fox jumped over the lazy dog");
    }
}
