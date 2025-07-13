using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     An interface to orchestrate across all available bank statements importers.
/// </summary>
public interface IBankStatementImporterRepository
{
    /// <summary>
    ///     Import the given file.
    ///     be imported by any of these importers a <see cref="NotSupportedException" /> will be thrown.
    /// </summary>
    Task<TransactionSetModel> ImportAsync(string fullFileName, Account account);
}
