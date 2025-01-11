namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A class to store configuration for the remote mobile data storage
/// </summary>
public class MobileStorageSettingsDto
{
    /// <summary>
    ///     The Amazon S3 Access Key Id identifying the user attempting to access the data.
    /// </summary>
    public required string AccessKeyId { get; init; }

    /// <summary>
    ///     The Amazon S3 Access Key Secret. This is effectively an access token replacement for a user password.
    /// </summary>
    public required string AccessKeySecret { get; init; }

    /// <summary>
    ///     The Amazon S3 region the data is stored in
    /// </summary>
    public required string AmazonS3Region { get; init; }
}
