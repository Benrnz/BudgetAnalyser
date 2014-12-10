using System.Threading.Tasks;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     The Statement File Manager is responsible for loading any kind of statement file, whether it be an existing budget
    ///     analyser statement file or a downloaded bank statement export to merge or load. If the filename is already known
    ///     it is loaded with no prompting, otherwise the user is prompted for a filename.
    ///     It also is responsible for saving  any open statement file into a budget analyser statement file.
    ///     To function it orchestrates across the  <see cref="IVersionedStatementModelRepository" /> and the
    ///     <see cref="IBankStatementImporterRepository" />.
    /// </summary>
    public interface IStatementRepository
    {
        /// <summary>
        ///     Imports a bank's transaction extract and merges it into an existing statement model. You cannot merge two Budget Analyser
        ///     Statement files.
        /// </summary>
        StatementModel ImportAndMergeBankStatementAsync(
            [NotNull] string storageKey, 
            [NotNull] StatementModel statementModel, [NotNull] AccountType account, 
            bool throwIfFileNotFound = false);

        /// <summary>
        ///     Loads either an existing Budget Analyser Statement model, or creates a new Budget Analyser Statement model from a
        ///     known bank statement file.
        /// </summary>
        /// <param name="storageKey">Pass a known storage key (database identifier or filename) to load.</param>
        Task<StatementModel> LoadStatementModelAsync([NotNull] string storageKey);

        /// <summary>
        ///     Save the given statement model into a file. The file will be created or updated and will be a Budget Analyser
        ///     Statement file format.
        ///     Saving and preserving bank statement files is not supported.
        /// </summary>
        Task SaveAsync(StatementModel statementModel);
    }
}