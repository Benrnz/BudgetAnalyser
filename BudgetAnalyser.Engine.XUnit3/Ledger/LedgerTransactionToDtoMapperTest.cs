#nullable disable
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class LedgerTransactionToDtoMapperTest
{
    private static readonly Guid TransactionId = new("7F921750-4467-4EA4-81E6-3EFD466341C6");

    public LedgerTransactionToDtoMapperTest()
    {
        TestData = new CreditLedgerTransaction(new Guid("7F921750-4467-4EA4-81E6-3EFD466341C6")) { Amount = 123.99M, Narrative = "Foo bar." };
        var subject = new MapperLedgerTransactionToDto2(new LedgerTransactionFactory());
        Result = subject.ToDto(TestData);
    }

    private LedgerTransactionDto Result { get; }
    private LedgerTransaction TestData { get; }

    [Fact]
    public void ShouldMapAmount()
    {
        Result.Amount.ShouldBe(123.99M);
    }

    [Fact]
    public void ShouldMapId()
    {
        Result.Id.ShouldBe(TransactionId);
    }

    [Fact]
    public void ShouldMapNarrative()
    {
        Result.Narrative.ShouldBe("Foo bar.");
    }

    [Fact]
    public void ShouldMapTransactionType()
    {
        Result.TransactionType.ShouldBe(typeof(CreditLedgerTransaction).FullName);
    }

    [Fact]
    public void ShouldNotMapBankAccountForCreditLedgerTransaction()
    {
        Result.Account.ShouldBeNull();
    }

    [Fact]
    public void ShouldNotMapBankAccountForDebitLedgerTransaction()
    {
        Result.Account.ShouldBeNull();
    }
}
