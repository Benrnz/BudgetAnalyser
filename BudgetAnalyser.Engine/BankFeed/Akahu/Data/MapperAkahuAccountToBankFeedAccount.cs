namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     Maps an Akahu account into the vendor-neutral <see cref="BankFeedAccount" />. This is a one way mapping.
/// </summary>
internal class MapperAkahuAccountToBankFeedAccount
{
    private const string ActiveStatus = "ACTIVE";
    private const string TransactionsAttribute = "TRANSACTIONS";

    /// <summary>
    ///     Returns true if Akahu can retrieve transactions for the given account.
    /// </summary>
    public static bool SupportsTransactions(AkahuAccountDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return dto.Attributes?.Contains(TransactionsAttribute, StringComparer.Ordinal) == true;
    }

    /// <summary>
    ///     Maps the given Akahu account.
    /// </summary>
    public BankFeedAccount ToModel(AkahuAccountDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new BankFeedAccount(
            dto.Id,
            dto.Connection?.Name ?? string.Empty,
            dto.Name ?? string.Empty,
            dto.FormattedAccount,
            string.Equals(dto.Status, ActiveStatus, StringComparison.Ordinal),
            dto.Refreshed?.Transactions);
    }
}
