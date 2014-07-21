namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<BudgetBucketDto, BudgetBucket>))]
    public class DtoToBudgetBucketMapper : MagicMapper<BudgetBucketDto, BudgetBucket>
    {
    }
}