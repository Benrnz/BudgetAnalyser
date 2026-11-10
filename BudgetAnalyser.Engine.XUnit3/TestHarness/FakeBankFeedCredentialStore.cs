using BudgetAnalyser.Engine.BankFeed;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

/// <summary>
///     An in-memory credential store for tests. Nothing is written to disk.
/// </summary>
internal sealed class FakeBankFeedCredentialStore : IBankFeedCredentialStore
{
    private readonly Dictionary<string, Dictionary<string, string>> values = new();
    private readonly Dictionary<string, DateTimeOffset?> verified = new();

    public void Clear(string providerKey)
    {
        this.values.Remove(providerKey);
        this.verified.Remove(providerKey);
    }

    public string? Get(string providerKey, string fieldKey)
    {
        return this.values.TryGetValue(providerKey, out var fields) && fields.TryGetValue(fieldKey, out var value) ? value : null;
    }

    public DateTimeOffset? GetConnectionVerified(string providerKey)
    {
        return this.verified.GetValueOrDefault(providerKey);
    }

    public void Save(string providerKey, IReadOnlyDictionary<string, string> newValues)
    {
        this.values[providerKey] = new Dictionary<string, string>(newValues);
        this.verified.Remove(providerKey);
    }

    public void SetConnectionVerified(string providerKey, DateTimeOffset? verifiedUtc)
    {
        this.verified[providerKey] = verifiedUtc;
    }
}
