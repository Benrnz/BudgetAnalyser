#nullable disable
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class DtoToLedgerTransactionMapperTest
{
    private static readonly Guid TransactionId = new("7F921750-4467-4EA4-81E6-3EFD466341C6");

    public DtoToLedgerTransactionMapperTest()
    {
        var subject = new MapperLedgerTransactionToDto2(new LedgerTransactionFactory());
        Result = subject.ToModel(TestData);
    }

    private LedgerTransaction Result { get; }

    private LedgerTransactionDto TestData { get; } = new(
        Id: TransactionId,
        Amount: -123.99M,
        Narrative: "Foo bar.",
        TransactionType: typeof(CreditLedgerTransaction).FullName,
        AutoMatchingReference: null,
        Date: null,
        Account: null);

    [Fact]
    public void ShouldMapAmount()
    {
        Result.Amount.ShouldBe(-123.99M);
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
        Result.ShouldBeOfType<CreditLedgerTransaction>();
    }
}
