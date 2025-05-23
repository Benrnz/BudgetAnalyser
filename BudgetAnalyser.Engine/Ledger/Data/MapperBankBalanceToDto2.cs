﻿using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Ledger.Data;

internal class MapperBankBalanceToDto2(IAccountTypeRepository accountTypeRepository) : IDtoMapper<BankBalanceDto, BankBalance>
{
    private readonly IAccountTypeRepository accountTypeRepo = accountTypeRepository ?? throw new ArgumentNullException(nameof(accountTypeRepository));

    public BankBalanceDto ToDto(BankBalance model)
    {
        var dto = new BankBalanceDto(model.Account.Name, model.Balance);
        return dto;
    }

    public BankBalance ToModel(BankBalanceDto dto)
    {
        var account = this.accountTypeRepo.GetByKey(dto.Account) ??
                      throw new CorruptedLedgerBookException($"Account not found for key '{dto.Account}'. It appears the ledger contains data not compatible with the budget account data.");
        var bankBalance = new BankBalance(account, dto.Balance);
        return bankBalance;
    }
}
