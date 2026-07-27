namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for <see cref="LedgerEntry" />.
/// </summary>
public record LedgerEntryDto(decimal Balance, string BucketCode, string StoredInAccount, LedgerTransactionDto[] Transactions);
