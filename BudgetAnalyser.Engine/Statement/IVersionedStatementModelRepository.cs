using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     This interface describes persistence functions for retrieving and saving <see cref="StatementModel" />. It will
    ///     only
    ///     load and save in the proprietry Budget Analyser Transaction format.
    ///     Rather than consuming this interface in a client, prefer to use the <see cref="IStatementRepository" /> instead, it
    ///     supports Budget Analyser Transaction files as well as Bank Extract files.
    /// </summary>
    public interface IVersionedStatementModelRepository
    {
        /// <summary>
        ///     Creates a new empty <see cref="StatementModel" /> at the location indicated by the <see cref="storageKey" />. Any
        ///     existing data at this location will be overwritten. After this is complete, use the <see cref="LoadAsync" /> method
        ///     to
        ///     load the new <see cref="StatementModel" />.
        /// </summary>
        Task CreateNewAndSaveAsync([NotNull] string storageKey);

        Task<bool> IsStatementModelAsync([NotNull] string storageKey);
        Task<StatementModel> LoadAsync([NotNull] string storageKey);
        Task SaveAsync([NotNull] StatementModel model, [NotNull] string storageKey);
    }
}