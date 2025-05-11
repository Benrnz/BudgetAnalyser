using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Budget.Data;

[AutoRegisterWithIoC]
public class MapperBudgetCollectionToDto2(IBudgetBucketRepository bucketRepo, IDtoMapper<BudgetModelDto, BudgetModel> budgetModelMapper) : IDtoMapper<BudgetCollectionDto, BudgetCollection>
{
    private readonly IDtoMapper<BudgetBucketDto, BudgetBucket> bucketMapper = new MapperBudgetBucketToDto2();
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
    private readonly IDtoMapper<BudgetModelDto, BudgetModel> budgetModelMapper = budgetModelMapper ?? throw new ArgumentNullException(nameof(budgetModelMapper));

    public BudgetCollectionDto ToDto(BudgetCollection model)
    {
        return new BudgetCollectionDto
        (
            this.bucketRepo.Buckets.Select(this.bucketMapper.ToDto).ToArray(),
            StorageKey: model.StorageKey,
            Budgets: model.Select(this.budgetModelMapper.ToDto).ToArray()
        );
    }

    public BudgetCollection ToModel(BudgetCollectionDto dto)
    {
        // Note budget buckets from the top of the DTO are not mapped here, as this mapper is only concerned with the BudgetCollection type.
        // Budget Buckets are created and mapped from the XamlOnDiskBudgetRepository directly into the InMemoryBudgetBucketRepository.
        var model = new BudgetCollection { StorageKey = dto.StorageKey };
        foreach (var budgetDto in dto.Budgets)
        {
            model.Add(this.budgetModelMapper.ToModel(budgetDto));
        }

        return model;
    }
}
