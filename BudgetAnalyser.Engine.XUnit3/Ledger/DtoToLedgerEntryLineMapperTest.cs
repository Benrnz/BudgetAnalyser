#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class DtoToLedgerEntryLineMapperTest
{
    public DtoToLedgerEntryLineMapperTest()
    {
        var accountRepo = new InMemoryAccountTypeRepository();
        var subject = new MapperLedgerEntryLineToDto2(accountRepo, new LedgerBucketFactory(new BudgetBucketRepoAlwaysFind(), accountRepo), new LedgerTransactionFactory());
        Result = subject.ToModel(TestData);
    }

    private LedgerEntryLine Result { get; }

    private LedgerEntryLineDto TestData
    {
        get
        {
            var book = LedgerBookDtoTestData.TestData3();
            return book.Reconciliations.First();
        }
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
        Result.Date.ShouldBe(new DateOnly(2014, 2, 20));
    }

    [Fact]
    public void ShouldMapEntries()
    {
        Result.Entries.Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldMapLedgerBalance()
    {
        Result.LedgerBalance.ShouldBe(1902.44M);
    }

    [Fact]
    public void ShouldMapRemarks()
    {
        Result.Remarks.ShouldBe("Lorem ipsum dolor. Mit solo darte.");
    }

    [Fact]
    public void ShouldMapSurplus()
    {
        Result.CalculatedSurplus.ShouldBe(1498.99M);
    }

    [Fact]
    public void ShouldMapTotalBalanceAdjustments()
    {
        Result.TotalBalanceAdjustments.ShouldBe(-100.01M);
    }

    [Fact]
    public void ShouldMapTotalBankBalance()
    {
        Result.TotalBankBalance.ShouldBe(2002.45M);
    }

    [Fact]
    public void ShouldSetIsNewToFalse()
    {
        Result.IsNew.ShouldBeFalse();
    }
}
