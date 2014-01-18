namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetModelImporter
    {
        IBudgetBucketRepository BudgetBucketRepository { get; }
        BudgetCollection LoadBudgetData(string fileName);

        void SaveBudgetData(BudgetCollection budget);
    }
}