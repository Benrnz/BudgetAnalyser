using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Statement.Data;

/// <summary>
///     A Dto to persist a set of statement transactions.
/// </summary>
public class TransactionSetDto
{
    /// <summary>
    ///     Gets or sets the checksum to help ensure consistency and detect persistence bugs.
    /// </summary>
    public long Checksum { get; set; }

    /// <summary>
    ///     Gets or sets the last import date and time.
    /// </summary>
    public DateTime LastImport { get; init; }

    /// <summary>
    ///     Gets or sets the storage key.
    /// </summary>
    public required string StorageKey { get; set; }

    /// <summary>
    ///     Gets or sets the transactions collection. This must be a List for serialisation reasons.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Necessary for persistence")]
    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for persistence")]
    public required List<TransactionDto> Transactions { get; init; }

    /// <summary>
    ///     Gets or sets the version hash. Used to indicate different persistence formats.
    /// </summary>
    public string VersionHash { get; set; } = Guid.Empty.ToString();
}
