namespace BudgetAnalyser.Engine.Transactions.Data;

/// <summary>
///     A Dto to persist a set of bank transactions.
/// </summary>
public record TransactionSetDto(DateTime LastImport, string StorageKey, TransactionDto[] Transactions, long Checksum = 0, string VersionHash = "00000000-0000-0000-0000-000000000000");
