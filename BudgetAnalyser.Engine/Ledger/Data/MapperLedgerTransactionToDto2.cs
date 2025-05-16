using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Ledger.Data;

internal class MapperLedgerTransactionToDto2(ILedgerTransactionFactory ledgerTransactionFactory) : IDtoMapper<LedgerTransactionDto, LedgerTransaction>
{
    private readonly ILedgerTransactionFactory transactionFactory = ledgerTransactionFactory ?? throw new ArgumentNullException(nameof(ledgerTransactionFactory));

    public LedgerTransactionDto ToDto(LedgerTransaction model)
    {
        if (model is BankBalanceAdjustmentTransaction)
        {
            throw new DataFormatException($"Please use the {nameof(MapperBankBalanceAdjustmentToDto2)} for BankBalanceAdjustmentTransaction.");
        }

        var dto = new LedgerTransactionDto
        (
            null, // Only used for type BalanceAdjustmentTransaction
            model.Amount,
            model.AutoMatchingReference,
            model.Date,
            model.Id,
            model.Narrative,
            model.GetType().FullName
        );
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
