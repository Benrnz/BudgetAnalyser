#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class DtoToBankBalanceMapperTest
{
    public DtoToBankBalanceMapperTest()
    {
        var subject = new MapperBankBalanceToDto2(new InMemoryAccountTypeRepository());
        Result = subject.ToModel(TestData);
    }

    private BankBalance Result { get; }

    private BankBalanceDto TestData => new(TransactionsListModelTestData.ChequeAccount.Name, 44552.44M);

    [Fact]
    public void ShouldMapAmount()
    {
        Result.Balance.ShouldBe(44552.44M);
    }

    [Fact]
    public void ShouldMapBankAccount()
    {
        Result.Account.Name.ShouldBe(TransactionsListModelTestData.ChequeAccount.Name);
    }
}
