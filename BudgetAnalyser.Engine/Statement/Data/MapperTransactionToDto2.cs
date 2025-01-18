using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Statement.Data;

public class MapperTransactionToDto2(IAccountTypeRepository accountRepo, IBudgetBucketRepository bucketRepo, ITransactionTypeRepository transactionTypeRepo) : IDtoMapper<TransactionDto, Transaction>
{
    private readonly IAccountTypeRepository accountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
    private readonly ITransactionTypeRepository transactionTypeRepo = transactionTypeRepo ?? throw new ArgumentNullException(nameof(transactionTypeRepo));

    public TransactionDto ToDto(Transaction model)
    {
        var dto = new TransactionDto
        {
            Account = model.Account.Name,
            Amount = model.Amount,
            BudgetBucketCode = model.BudgetBucket?.Code,
            Date = model.Date,
            Description = model.Description,
            Id = model.Id,
            Reference1 = model.Reference1,
            Reference2 = model.Reference2,
            Reference3 = model.Reference3,
            TransactionType = model.TransactionType.Name
        };
        return dto;
    }

    public Transaction ToModel(TransactionDto dto)
    {
        var txn = new Transaction
        {
            Account = this.accountRepo.GetByKey(dto.Account) ?? throw new NotSupportedException($"Statement file data contains unsupported Account '{dto.Account}"),
            Amount = dto.Amount,
            Date = dto.Date,
            Description = dto.Description,
            BudgetBucket = string.IsNullOrWhiteSpace(dto.BudgetBucketCode)
                ? null
                : this.bucketRepo.GetByCode(dto.BudgetBucketCode) ??
                  throw new NotSupportedException($"Statement file data contains a unsupported BucketCode '{dto.BudgetBucketCode}' compared to Budget file."),
            Id = dto.Id,
            Reference1 = dto.Reference1,
            Reference2 = dto.Reference2,
            Reference3 = dto.Reference3,
            TransactionType = this.transactionTypeRepo.GetOrCreateNew(dto.TransactionType)
        };

        return txn;
    }
}
