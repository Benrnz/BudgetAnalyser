using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     The Statement File Manager is responsible for loading any kind of statement file, whether it be an existing budget
    ///     analyser statement file or a downloaded bank statement export to merge or load. If the filename is already known
    ///     it is loaded with no prompting, otherwise the user is prompted for a filename.
    ///     It also is responsible for saving  any open statement file into a budget analyser statement file.
    ///     To function it orchestrates across the  <see cref="IVersionedStatementModelRepository" /> and the
    ///     <see cref="IBankStatementImporterRepository" />.
    /// </summary>
    public interface IStatementFileManager
    {
        /// <summary>
        ///     Imports a bank statement file and merges it into an existing statement model. You cannot merge two Budget Analyser
        ///     Statement files.
        /// </summary>
        StatementModel ImportAndMergeBankStatement(StatementModel statementModel, bool throwIfFileNotFound = false);

        /// <summary>
        ///     Loads either an existing Budget Analyser Statement file, or creates a new Budget Analyser Statement file from a
        ///     known bank statement
        ///     file.
        /// </summary>
        /// <param name="fileName">Pass a known filename to load or null to prompt the user to choose a file.</param>
        StatementModel LoadAnyStatementFile(string fileName);

        /// <summary>
        ///     Save the given statement model into a file. The file will be created or updated and will be a Budget Analyser
        ///     Statement file format.
        ///     Saving and preserving bank statement files is not supported.
        /// </summary>
        void Save(StatementModel statementModel);
    }
}