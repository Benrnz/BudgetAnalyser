using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    public interface IVersionedStatementModelRepository
    {
        Task CreateNewAsync([NotNull] string storageKey);
        Task<bool> IsStatementModelAsync([NotNull] string storageKey);
        Task<StatementModel> LoadAsync([NotNull] string storageKey);
        Task SaveAsync([NotNull] StatementModel model, [NotNull] string storageKey);
    }
}