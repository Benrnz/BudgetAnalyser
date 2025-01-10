using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Ledger.Data.Data2;

internal class MapperLedgerTransactionToDto2(ILedgerTransactionFactory ledgerTransactionFactory) : IDtoMapper<LedgerTransactionDto, LedgerTransaction>
{
    private readonly ILedgerTransactionFactory transactionFactory = ledgerTransactionFactory ?? throw new ArgumentNullException(nameof(ledgerTransactionFactory));

    public LedgerTransactionDto ToDto(LedgerTransaction model)
    {
        if (model is BankBalanceAdjustmentTransaction)
        {
            throw new NotSupportedException($"Please use the {nameof(MapperBankBalanceAdjustmentToDto2)} for BankBalanceAdjustmentTransaction.");
        }

        var dto = new LedgerTransactionDto
        {
            Account = null, // Only used for type BalanceAdjustmentTransaction
            Amount = model.Amount,
            AutoMatchingReference = model.AutoMatchingReference,
            Date = model.Date,
            Id = model.Id,
            Narrative = model.Narrative,
            TransactionType = model.GetType().FullName
        };
        return dto;
    }

    public LedgerTransaction ToModel(LedgerTransactionDto dto)
    {
        if (dto.TransactionType is null || string.IsNullOrWhiteSpace(dto.TransactionType))
        {
            throw new CorruptedLedgerBookException($"The transaction type is missing for transaction {dto.Id}");
        }

        var transaction = this.transactionFactory.Build(dto.TransactionType, dto.Id);
        transaction.Amount = dto.Amount;
        transaction.Narrative = dto.Narrative ?? string.Empty;
        transaction.AutoMatchingReference = dto.AutoMatchingReference ?? string.Empty;
        transaction.Date = dto.Date;

        return transaction;
    }
}
