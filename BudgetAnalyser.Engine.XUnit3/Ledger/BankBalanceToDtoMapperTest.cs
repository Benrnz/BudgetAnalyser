#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class BankBalanceToDtoMapperTest
{
    public BankBalanceToDtoMapperTest()
    {
        var subject = new MapperBankBalanceToDto2(new InMemoryAccountTypeRepository());
        Result = subject.ToDto(TestData);
    }

    private BankBalanceDto Result { get; }
    private BankBalance TestData => new(TransactionsListModelTestData.ChequeAccount, 44552.21M);

    [Fact]
    public void ShouldMapAmount()
    {
        Result.Balance.ShouldBe(44552.21M);
    }

    [Fact]
    public void ShouldMapBankAccount()
    {
        Result.Account.ShouldBe(TransactionsListModelTestData.ChequeAccount.Name);
    }
}
