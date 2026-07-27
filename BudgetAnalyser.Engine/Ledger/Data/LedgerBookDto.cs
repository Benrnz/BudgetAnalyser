namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for <see cref="LedgerBook" />
/// </summary>
public record LedgerBookDto(
    double Checksum,
    LedgerBucketDto[] Ledgers,
    MobileStorageSettingsDto? MobileSettings,
    DateTime Modified,
    string Name,
    LedgerEntryLineDto[] Reconciliations,
    string StorageKey);
