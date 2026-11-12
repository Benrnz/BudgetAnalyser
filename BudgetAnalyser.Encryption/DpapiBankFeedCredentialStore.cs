using System.Security.Cryptography;
using System.Text.Json;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankFeed;

namespace BudgetAnalyser.Encryption;

/// <summary>
///     Stores bank feed credentials in a file encrypted with Windows DPAPI (current user scope). Only the same Windows user on the same computer can decrypt it.
///     The file is stored in the user's local (not roaming) application data folder, outside of the repository and outside of the budget data files. It is not roamed
///     because it cannot be decrypted on another computer. Values are never logged.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal sealed class DpapiBankFeedCredentialStore : IBankFeedCredentialStore
{
    private const string FileName = "BankFeedCredentials.dat";

    private readonly string filePath;
    private readonly ILogger logger;
    private readonly Lock syncRoot = new();
    private Dictionary<string, StoredProvider>? cache;

    public DpapiBankFeedCredentialStore(ILogger logger)
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BudgetAnalyser", FileName), logger)
    {
    }

    /// <summary>
    ///     Used by unit tests to store the file in a temporary location.
    /// </summary>
    internal DpapiBankFeedCredentialStore(string filePath, ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        this.filePath = filePath;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Clear(string providerKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        lock (this.syncRoot)
        {
            var data = Load();
            if (data.Remove(providerKey))
            {
                Write(data);
            }
        }
    }

    public string? Get(string providerKey, string fieldKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldKey);
        lock (this.syncRoot)
        {
            return Load().TryGetValue(providerKey, out var provider) && provider.Values.TryGetValue(fieldKey, out var value) ? value : null;
        }
    }

    public DateTimeOffset? GetConnectionVerified(string providerKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        lock (this.syncRoot)
        {
            return Load().TryGetValue(providerKey, out var provider) ? provider.ConnectionVerifiedUtc : null;
        }
    }

    public void Save(string providerKey, IReadOnlyDictionary<string, string> values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        ArgumentNullException.ThrowIfNull(values);
        lock (this.syncRoot)
        {
            var data = Load();

            // New values have not been tested, so the connection verified time is cleared.
            data[providerKey] = new StoredProvider { Values = new Dictionary<string, string>(values) };
            Write(data);
        }
    }

    public void SetConnectionVerified(string providerKey, DateTimeOffset? verifiedUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        lock (this.syncRoot)
        {
            var data = Load();
            if (!data.TryGetValue(providerKey, out var provider))
            {
                if (verifiedUtc is null)
                {
                    return;
                }

                provider = new StoredProvider();
                data[providerKey] = provider;
            }

            provider.ConnectionVerifiedUtc = verifiedUtc;
            Write(data);
        }
    }

    private static byte[] Protect(byte[] data)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Bank feed credentials can only be stored on Windows.");
        }

        return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
    }

    private static byte[] Unprotect(byte[] data)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Bank feed credentials can only be read on Windows.");
        }

        return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
    }

    private Dictionary<string, StoredProvider> Load()
    {
        if (this.cache is not null)
        {
            return this.cache;
        }

        if (!File.Exists(this.filePath))
        {
            this.cache = new Dictionary<string, StoredProvider>();
            return this.cache;
        }

        try
        {
            var json = Unprotect(File.ReadAllBytes(this.filePath));
            this.cache = JsonSerializer.Deserialize<Dictionary<string, StoredProvider>>(json) ?? new Dictionary<string, StoredProvider>();
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException)
        {
            // For example the file was created by a different Windows user. The credentials must be entered again; the unreadable file is overwritten on the next save.
            this.logger.LogWarning(_ => $"The bank feed credentials file could not be read and will be ignored. {ex.GetType().Name}");
            this.cache = new Dictionary<string, StoredProvider>();
        }

        return this.cache;
    }

    private void Write(Dictionary<string, StoredProvider> data)
    {
        try
        {
            var folder = Path.GetDirectoryName(this.filePath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var encrypted = Protect(JsonSerializer.SerializeToUtf8Bytes(data));

            // Write to a temporary file first so a failure part way through never leaves a corrupt credentials file.
            var tempFile = this.filePath + ".tmp";
            File.WriteAllBytes(tempFile, encrypted);
            File.Move(tempFile, this.filePath, true);
            this.cache = data;
        }
        catch
        {
            // The callers change the cached data before writing it. If the write fails the cache no longer matches the file, so it is discarded and read from the file again
            // the next time it is needed.
            this.cache = null;
            throw;
        }
    }

    private sealed class StoredProvider
    {
        public DateTimeOffset? ConnectionVerifiedUtc { get; set; }
        public Dictionary<string, string> Values { get; set; } = new();
    }
}
