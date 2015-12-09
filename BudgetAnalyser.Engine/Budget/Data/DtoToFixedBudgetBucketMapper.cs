namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<FixedBudgetBucketDto, FixedBudgetProjectBucket>))]
    public class DtoToFixedBudgetBucketMapper : MagicMapper<FixedBudgetBucketDto, FixedBudgetProjectBucket>
    {
    }
}