using Rees.TangyFruitMapper;

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
        {
            Buckets = this.bucketRepo.Buckets.Select(this.bucketMapper.ToDto).ToList(),
            StorageKey = model.StorageKey,
            Budgets = model.Select(this.budgetModelMapper.ToDto).ToList()
        };
    }

    public BudgetCollection ToModel(BudgetCollectionDto dto)
    {
        var model = new BudgetCollection { StorageKey = dto.StorageKey };
        dto.Buckets.ForEach(x => this.bucketRepo.GetOrCreateNew(x.Code, () => this.bucketMapper.ToModel(x)));
        dto.Budgets.ForEach(x => model.Add(this.budgetModelMapper.ToModel(x)));
        return model;
    }
}
