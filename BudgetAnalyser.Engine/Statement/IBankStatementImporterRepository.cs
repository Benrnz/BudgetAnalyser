using System;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An interface to orchestrate across all available bank statements importers.
    /// </summary>
    public interface IBankStatementImporterRepository
    {
        /// <summary>
        ///     Can any importer in this repository read and import the given file.
        ///     Calling this method will open the file and read some of its contents.
        /// </summary>
        /// <returns>True if this file can be imported one of the importers in the repository.</returns>
        Task<bool> CanImportAsync(string fullFileName);

        /// <summary>
        ///     Import the given file.  It is recommended to call <see cref="CanImportAsync" /> first.  If the file cannot
        ///     be imported by any of this repositories importers a <see cref="NotSupportedException" /> will be thrown.
        /// </summary>
        Task<StatementModel> ImportAsync(string fullFileName, Account.Account account);
    }
}