using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetRepository
    {
        Task<BudgetCollection> CreateNewAsync([NotNull] string fileName);
        // TODO do we really still need this CreateNew()?
        BudgetCollection CreateNew();
        Task<BudgetCollection> LoadAsync([NotNull] string fileName);

        Task SaveAsync();
    }
}