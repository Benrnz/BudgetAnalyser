#nullable disable
using BudgetAnalyser.Engine.Ledger;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class CreditLedgerTransactionTest
{
    [Fact]
    public void ConstructWithGuidShouldSetId()
    {
        var id = Guid.NewGuid();
        var subject = new CreditLedgerTransaction(id);

        subject.Id.ShouldBe(id);
    }

    [Fact]
    public void WithAmount50ShouldCreateACreditOf50()
    {
        var subject = new CreditLedgerTransaction();

        var result = subject.WithAmount(50);

        result.Amount.ShouldBe(50M);
    }

    [Fact]
    public void WithAmount50ShouldReturnSameObjectForChaining()
    {
        var subject = new CreditLedgerTransaction();

        var result = subject.WithAmount(50);

        result.ShouldBeSameAs(subject);
    }

    [Fact]
    public void WithAmountMinus50ShouldCreateADebitOf50()
    {
        var subject = new CreditLedgerTransaction();

        var result = subject.WithAmount(-50);

        result.Amount.ShouldBe(-50M);
    }
}
