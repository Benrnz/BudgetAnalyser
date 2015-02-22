using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetRepository
    {
        IBudgetBucketRepository BudgetBucketRepository { get; }
        BudgetCollection CreateNew(string fileName);
        BudgetCollection CreateNew();
        Task<BudgetCollection> LoadAsync(string fileName);
        void Save(BudgetCollection budget);
        void Save();
    }
}