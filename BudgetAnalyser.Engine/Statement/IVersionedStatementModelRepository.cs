using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Statement
{
    public interface IVersionedStatementModelRepository
    {
        Task<bool> IsValidFileAsync(string fileName);

        Task<StatementModel> LoadAsync(string fileName);

        Task SaveAsync(StatementModel model, string fileName);
    }
}