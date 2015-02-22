using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Statement
{
    public interface IVersionedStatementModelRepository
    {
        Task<bool> IsStatementModelAsync(string storageKey);

        Task<StatementModel> LoadAsync(string storageKey);

        Task SaveAsync(StatementModel model, string storageKey);
    }
}