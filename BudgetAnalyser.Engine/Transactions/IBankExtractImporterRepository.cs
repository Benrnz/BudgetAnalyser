using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Transactions;

/// <summary>
///     An interface to orchestrate across all available bank extract importers.
/// </summary>
public interface IBankExtractImporterRepository
{
    /// <summary>
    ///     Import the given file. If the file cannot be imported by any of these importers a <see cref="NotSupportedException" /> will be thrown.
    /// </summary>
    Task<TransactionsListModel> ImportAsync(string fullFileName, Account account);
}
