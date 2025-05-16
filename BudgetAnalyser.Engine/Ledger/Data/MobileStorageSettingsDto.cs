namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A class to store configuration for the remote mobile data storage
/// </summary>
public record MobileStorageSettingsDto(string AccessKeyId, string AccessKeySecret, string AmazonS3Region);
