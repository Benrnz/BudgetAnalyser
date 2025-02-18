using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Ledger.Data;

internal class MapperLedgerBucketToDto2(IBudgetBucketRepository bucketRepo, IAccountTypeRepository accountTypeRepo, ILedgerBucketFactory bucketFactory)
    : IDtoMapper<LedgerBucketDto, LedgerBucket>
{
    private readonly IAccountTypeRepository accountTypeRepo = accountTypeRepo ?? throw new ArgumentNullException(nameof(accountTypeRepo));
    private readonly ILedgerBucketFactory bucketFactory = bucketFactory ?? throw new ArgumentNullException(nameof(bucketFactory));
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));

    public LedgerBucketDto ToDto(LedgerBucket model)
    {
        var dto = new LedgerBucketDto { BucketCode = model.BudgetBucket.Code, StoredInAccount = model.StoredInAccount.Name };
        return dto;
    }

    public LedgerBucket ToModel(LedgerBucketDto dto)
    {
        var storedInAccount = this.accountTypeRepo.GetByKey(dto.StoredInAccount) ??
                              throw new CorruptedLedgerBookException(
                                  $"Account not found for key '{dto.StoredInAccount}'. It appears the ledger contains data not compatible with the budget account data.");
        var budgetBucket = this.bucketRepo.GetByCode(dto.BucketCode);
        var ledgerBucket = this.bucketFactory.Build(dto.BucketCode, storedInAccount);
        ledgerBucket.BudgetBucket = budgetBucket ??
                                    throw new CorruptedLedgerBookException(
                                        $"Budget bucket not found for code '{dto.BucketCode}'. It appears the ledger contains data not compatible with the budget data.");

        return ledgerBucket;
    }
}
