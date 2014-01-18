using System;
using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    /// An interface to orchestrate across all available bank statements importers.
    /// </summary>
    public interface IBankStatementImporterRepository
    {
        /// <summary>
        /// Can any importer in this repository read and import the given file.
        /// Calling this method will open the file and read some of its contents.
        /// </summary>
        /// <returns>True if this file can be imported one of the importers in the repository.</returns>
        bool CanImport(string fullFileName);

        /// <summary>
        /// Import the given file.  It is recommended to call <see cref="CanImport"/> first.  If the file cannot
        /// be imported by any of this repositories importers a <see cref="NotSupportedException"/> will be thrown.
        /// </summary>
        StatementModel Import(string fullFileName, AccountType accountType);
    }
}