using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Budget.Data;

[AutoRegisterWithIoC]
internal partial class MapperBudgetCollectionDtoBudgetCollection(
    IBudgetBucketRepository bucketRepo,
    IDtoMapper<BudgetBucketDto, BudgetBucket> bucketMapper,
    IDtoMapper<BudgetModelDto, BudgetModel> budgetMapper)
{
    private readonly IDtoMapper<BudgetBucketDto, BudgetBucket> bucketMapper = bucketMapper ?? throw new ArgumentNullException(nameof(bucketMapper));
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
    private readonly IDtoMapper<BudgetModelDto, BudgetModel> budgetMapper = budgetMapper ?? throw new ArgumentNullException(nameof(budgetMapper));

    partial void ToDtoPostprocessing(ref BudgetCollectionDto dto, BudgetCollection model)
    {
        dto.Buckets = this.bucketRepo.Buckets.Select(b => this.bucketMapper.ToDto(b)).ToList();
        dto.Budgets = model.ToList().Select(x => this.budgetMapper.ToDto(x)).ToList();
    }

    partial void ToModelPostprocessing(BudgetCollectionDto dto, ref BudgetCollection model)
    {
        var budgetCollection = model;
        dto.Budgets.ForEach(x => budgetCollection.Add(this.budgetMapper.ToModel(x)));
        dto.Buckets.ForEach(x => this.bucketRepo.GetOrCreateNew(x.Code, () => this.bucketMapper.ToModel(x)));
    }
}
