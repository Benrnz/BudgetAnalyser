using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Statement.Data;

[AutoRegisterWithIoC]
public class MapperTransactionsListModelToDto2(
    IAccountTypeRepository accountRepo,
    IBudgetBucketRepository bucketRepo,
    ITransactionTypeRepository transactionTypeRepo,
    ILogger logger) : IDtoMapper<TransactionSetDto, TransactionsListModel>
{
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDtoMapper<TransactionDto, Transaction> transactionMapper = new MapperTransactionToDto2(accountRepo, bucketRepo, transactionTypeRepo);

    public TransactionSetDto ToDto(TransactionsListModel model)
    {
        this.logger.LogInfo(_ => $"Mapping TransactionsListModel '{model.StorageKey}' into a DTO.");
        var dto = new TransactionSetDto
        (
            model.LastImport.ToUniversalTime(),
            model.StorageKey,
            model.AllTransactions.Select(this.transactionMapper.ToDto).ToArray(),
            0
        );
        // Checksum and VersionHash are set during persistence
        this.logger.LogInfo(_ => $"Mapping of TransactionsListModel '{model.StorageKey}' finished. {dto.Transactions.Count()} of {model.AllTransactions.Count()} exported.");
        return dto;
    }

    public TransactionsListModel ToModel(TransactionSetDto dto)
    {
        this.logger.LogInfo(_ => $"Mapping TransactionSetDto '{dto.StorageKey}' into a TransactionsListModel.");
        var model = new TransactionsListModel(this.logger) { StorageKey = dto.StorageKey, LastImport = dto.LastImport.ToLocalTime() };
        model.LoadTransactions(dto.Transactions.Select(t => this.transactionMapper.ToModel(t)));
        this.logger.LogInfo(_ => $"Mapping of TransactionSetDto '{dto.StorageKey}' finished. {model.AllTransactions.Count()} of {dto.Transactions.Count()} imported.");
        return model;
    }
}
