using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetRepository
    {
        IBudgetBucketRepository BudgetBucketRepository { get; }
        BudgetCollection CreateNew(string fileName);
        BudgetCollection CreateNew();
        Task<BudgetCollection> LoadAsync(string fileName);

        // TODO I dont think we need both of these save methods.
        Task SaveAsync(BudgetCollection budget);
        Task SaveAsync();
    }
}