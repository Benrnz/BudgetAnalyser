namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetBucket, BudgetBucketDto>))]
    public class BudgetBucketToDtoMapper : MagicMapper<BudgetBucket, BudgetBucketDto>
    {
    }
}