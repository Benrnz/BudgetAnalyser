using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Statement.Data;

[AutoRegisterWithIoC]
public class MapperStatementModelToDto2(
    IAccountTypeRepository accountRepo,
    IBudgetBucketRepository bucketRepo,
    ITransactionTypeRepository transactionTypeRepo,
    ILogger logger) : IDtoMapper<TransactionSetDto, StatementModel>
{
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDtoMapper<TransactionDto, Transaction> transactionMapper = new MapperTransactionToDto2(accountRepo, bucketRepo, transactionTypeRepo);

    public TransactionSetDto ToDto(StatementModel model)
    {
        this.logger.LogInfo(_ => $"Mapping StatementModel '{model.StorageKey}' into a DTO.");
        var dto = new TransactionSetDto
        {
            LastImport = model.LastImport.ToUniversalTime(),
            StorageKey = model.StorageKey,
            Transactions = model.AllTransactions.Select(this.transactionMapper.ToDto).ToList()
        };
        // Checksum and VersionHash are set during persistence
        this.logger.LogInfo(_ => $"Mapping of StatementModel '{model.StorageKey}' finished. {dto.Transactions.Count()} of {model.AllTransactions.Count()} exported.");
        return dto;
    }

    public StatementModel ToModel(TransactionSetDto dto)
    {
        this.logger.LogInfo(_ => $"Mapping TransactionSetDto '{dto.StorageKey}' into a StatementModel.");
        var model = new StatementModel(this.logger) { StorageKey = dto.StorageKey, LastImport = dto.LastImport.ToLocalTime() };
        model.LoadTransactions(dto.Transactions.Select(t => this.transactionMapper.ToModel(t)));
        this.logger.LogInfo(_ => $"Mapping of TransactionSetDto '{dto.StorageKey}' finished. {model.AllTransactions.Count()} of {dto.Transactions.Count()} imported.");
        return model;
    }
}
