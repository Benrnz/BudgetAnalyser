using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     The outcome of downloading and merging bank feed transactions.
/// </summary>
/// <param name="Range">The date range that was downloaded.</param>
/// <param name="Accounts">The outcome for each account that was downloaded.</param>
/// <param name="SuspectedDuplicates">
///     The number of transactions in the downloaded date range that are flagged as suspected duplicates after the merge. These are left in place for the user to review.
/// </param>
public record BankFeedImportResult(ImportDateRange Range, IReadOnlyList<BankFeedAccountImportResult> Accounts, int SuspectedDuplicates)
{
    /// <summary>
    ///     Gets the total number of new transactions added across all accounts.
    /// </summary>
    public int TotalImported => Accounts.Sum(a => a.Imported);
}

/// <summary>
///     The outcome of downloading one account.
/// </summary>
/// <param name="Account">The Budget Analyser account the transactions were imported into.</param>
/// <param name="Downloaded">The number of transactions downloaded.</param>
/// <param name="Imported">The number of new transactions added.</param>
/// <param name="AlreadyImported">The number of downloaded transactions skipped because they had already been imported.</param>
public record BankFeedAccountImportResult(Account Account, int Downloaded, int Imported, int AlreadyImported);
