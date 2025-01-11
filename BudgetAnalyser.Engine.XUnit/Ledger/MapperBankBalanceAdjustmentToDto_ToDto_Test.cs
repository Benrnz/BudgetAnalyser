using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using FluentAssertions;
using NSubstitute;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class MapperBankBalanceAdjustmentToDto_ToDto_Test
{
    private const string AccountName = "CHEQUE";
    private static readonly Guid TransactionId = new("7F921750-4467-4EA4-81E6-3EFD466341A6");
    private readonly MapperBankBalanceAdjustmentToDto2 subject;
    private readonly BankBalanceAdjustmentTransaction testData;
    private LedgerTransactionDto result;

    public MapperBankBalanceAdjustmentToDto_ToDto_Test()
    {
        var account = new ChequeAccount(AccountName);
        this.testData = new BankBalanceAdjustmentTransaction(TransactionId) { Amount = 123.99M, Narrative = "Foo bar.", BankAccount = account, Date = new DateTime(2025, 1, 11) };
        var accountRepo = Substitute.For<IAccountTypeRepository>();
        accountRepo.GetByKey(AccountName).Returns(account);
        this.subject = new MapperBankBalanceAdjustmentToDto2(accountRepo);
    }

    [Fact]
    public void ShouldMapAmount()
    {
        Act();
        this.result.Amount.Should().Be(123.99M);
    }

    [Fact]
    public void ShouldMapBankAccount()
    {
        Act();
        this.result.Account.Should().Be(AccountName);
    }

    [Fact]
    public void ShouldMapId()
    {
        Act();
        this.result.Id.Should().Be(TransactionId);
    }

    [Fact]
    public void ShouldMapNarrative()
    {
        Act();
        this.result.Narrative.Should().Be("Foo bar.");
    }

    [Fact]
    public void ShouldMapTransactionType()
    {
        Act();
        this.result.TransactionType.Should().Be(typeof(BankBalanceAdjustmentTransaction).FullName);
    }

    private void Act()
    {
        this.result = this.subject.ToDto(this.testData);
    }
}
