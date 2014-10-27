using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Statement
{
    public interface IVersionedStatementModelRepository
    {
        Task<bool> IsValidFileAsync(string fileName);

        Task<StatementModel> LoadAsync(string fileName);

        void Save(StatementModel model, string fileName);
    }
}