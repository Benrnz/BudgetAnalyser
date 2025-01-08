namespace BudgetAnalyser.Engine.Budget.Data;

[AutoRegisterWithIoC]
internal partial class MapperBudgetBucketDtoBudgetBucket(IBudgetBucketFactory bucketFactory)
{
    private readonly IBudgetBucketFactory bucketFactory = bucketFactory ?? throw new ArgumentNullException(nameof(bucketFactory));

    // ReSharper disable once RedundantAssignment
    partial void DtoFactory(ref BudgetBucketDto dto, BudgetBucket model)
    {
        dto = this.bucketFactory.BuildDto(model);
    }

    // ReSharper disable once RedundantAssignment
    partial void ModelFactory(BudgetBucketDto dto, ref BudgetBucket model)
    {
        model = this.bucketFactory.BuildModel(dto);
    }
}
