using BudgetAnalyser.Engine.BankAccount;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Ledger.Data.Data2;

public class MapperBankBalanceAdjustmentToDto2(IAccountTypeRepository accountTypeRepository) : IDtoMapper<LedgerTransactionDto, BankBalanceAdjustmentTransaction>
{
    private readonly IAccountTypeRepository accountTypeRepo = accountTypeRepository ?? throw new ArgumentNullException(nameof(accountTypeRepository));

    public LedgerTransactionDto ToDto(BankBalanceAdjustmentTransaction model)
    {
        var dto = new LedgerTransactionDto
        {
            Account = model.BankAccount.Name,
            TransactionType = model.GetType().FullName,
            Amount = model.Amount,
            AutoMatchingReference = model.AutoMatchingReference,
            Date = model.Date,
            Id = model.Id,
            Narrative = model.Narrative
        };

        return dto;
    }

    public BankBalanceAdjustmentTransaction ToModel(LedgerTransactionDto dto)
    {
        if (dto.Account == null)
        {
            throw new ArgumentNullException(nameof(dto.Account));
        }

        var bankAccount = this.accountTypeRepo.GetByKey(dto.Account);
        var balanceAdjustment = new BankBalanceAdjustmentTransaction
        {
            Amount = dto.Amount,
            AutoMatchingReference = dto.AutoMatchingReference ?? string.Empty,
            Date = dto.Date,
            Id = dto.Id,
            Narrative = dto.Narrative ?? string.Empty,
            BankAccount = bankAccount ??
                          throw new CorruptedLedgerBookException($"Account not found for '{dto.Account}'. It appears the ledger contains data not compatible with the budget account data.")
        };

        return balanceAdjustment;
    }
}
