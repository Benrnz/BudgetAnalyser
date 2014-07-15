namespace BudgetAnalyser.Engine.Budget.Data
{
    public interface IBudgetBucketFactory
    {
        BudgetBucket Build(BucketDtoType type);
        BucketDtoType SerialiseType(BudgetBucket bucket);
    }
}