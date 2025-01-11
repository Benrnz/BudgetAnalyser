using System.Xml.Serialization;

namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for <see cref="LedgerBook" />
/// </summary>
public class LedgerBookDto
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LedgerBookDto" /> class.
    /// </summary>
    public LedgerBookDto()
    {
        Reconciliations = new List<LedgerEntryLineDto>();
        Ledgers = new List<LedgerBucketDto>();
    }

    /// <summary>
    ///     A hash value to verify once loaded from a file.
    /// </summary>
    public double Checksum { get; set; }

    /// <summary>
    ///     The ledger to Bucket mapping for when a new reconciliation creates a new instances of LedgerEntry's.
    /// </summary>
    public List<LedgerBucketDto> Ledgers { get; init; }

    /// <summary>
    ///     The configuration for the remote mobile data storage
    /// </summary>
    public MobileStorageSettingsDto? MobileSettings { get; init; }

    /// <summary>
    ///     Gets or sets the last modified date.
    /// </summary>
    [XmlAttribute]
    public DateTime Modified { get; set; }

    /// <summary>
    ///     Gets or sets the ledger book name.
    /// </summary>
    [XmlAttribute]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the reconciliations collection.
    /// </summary>
    public List<LedgerEntryLineDto> Reconciliations { get; init; }

    /// <summary>
    ///     Gets or sets the storage key.
    /// </summary>
    public string StorageKey { get; set; } = string.Empty;
}
