using System.Security.Cryptography;
using System.Text;

namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Creates name-based (version 5, SHA-1) UUIDs as defined by RFC 9562 section 5.5. The same namespace and name always produce the same <see cref="Guid" />.
///     This is used to give a downloaded bank feed transaction a stable <see cref="Transactions.Transaction.Id" />, so downloading it again can be recognised.
/// </summary>
public static class DeterministicGuid
{
    /// <summary>
    ///     The fixed namespace for all Budget Analyser bank feed transaction identifiers. This is not a secret, and must never change once used, otherwise previously imported
    ///     transactions will no longer be recognised.
    /// </summary>
    public static readonly Guid BankFeedNamespace = new("91464dac-b52c-4da9-aec1-a3795702cbf5");

    /// <summary>
    ///     Creates a version 5 UUID from the given namespace and name.
    /// </summary>
    public static Guid Create(Guid namespaceId, string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var nameBytes = Encoding.UTF8.GetBytes(name);
        var input = new byte[16 + nameBytes.Length];
        namespaceId.TryWriteBytes(input, true, out _);
        nameBytes.CopyTo(input, 16);

        var hash = SHA1.HashData(input);
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50); // Version 5
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // RFC 9562 variant
        return new Guid(hash.AsSpan(0, 16), true);
    }

    /// <summary>
    ///     Creates the stable identifier for a bank feed transaction. The provider key is included so two providers can never produce the same identifier.
    /// </summary>
    /// <param name="providerKey">The <see cref="IBankFeedProvider.ProviderKey" />.</param>
    /// <param name="externalTransactionId">The provider's identifier for the transaction, <see cref="BankFeedTransaction.ExternalId" />.</param>
    public static Guid CreateForBankFeedTransaction(string providerKey, string externalTransactionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalTransactionId);
        return Create(BankFeedNamespace, $"{providerKey}:{externalTransactionId}");
    }
}
