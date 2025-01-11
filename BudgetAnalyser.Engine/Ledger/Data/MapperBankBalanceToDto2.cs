using BudgetAnalyser.Engine.BankAccount;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Ledger.Data;

internal class MapperBankBalanceToDto2(IAccountTypeRepository accountTypeRepository) : IDtoMapper<BankBalanceDto, BankBalance>
{
    private readonly IAccountTypeRepository accountTypeRepo = accountTypeRepository ?? throw new ArgumentNullException(nameof(accountTypeRepository));

    public BankBalanceDto ToDto(BankBalance model)
    {
        var dto = new BankBalanceDto { Account = model.Account.Name, Balance = model.Balance };
        return dto;
    }

    public BankBalance ToModel(BankBalanceDto dto)
    {
        var account = this.accountTypeRepo.GetByKey(dto.Account);
        if (account is null)
        {
            throw new KeyNotFoundException($"Account not found for key '{dto.Account}'. It appears the ledger contains data not compatible with the budget account data.");
        }

        var bankBalance = new BankBalance(account, dto.Balance);
        return bankBalance;
    }
}
