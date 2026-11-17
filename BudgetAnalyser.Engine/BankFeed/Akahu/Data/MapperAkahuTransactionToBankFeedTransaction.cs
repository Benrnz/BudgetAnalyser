namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     Maps an Akahu transaction into the vendor-neutral <see cref="BankFeedTransaction" />. This is a one way mapping; bank feed data is never written back to Akahu.
/// </summary>
internal class MapperAkahuTransactionToBankFeedTransaction
{
    /// <summary>
    ///     Maps the given Akahu transaction.
    /// </summary>
    /// <param name="dto">The Akahu transaction.</param>
    /// <param name="institution">The name of the institution the transaction's account is held at. Akahu transactions do not include this, it comes from the account.</param>
    public BankFeedTransaction ToModel(AkahuTransactionDto dto, string institution)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(institution);

        return new BankFeedTransaction(
            dto.Id,
            dto.Account,
            institution,
            AkahuDates.ToNewZealandDate(dto.Date),
            dto.Description ?? string.Empty,
            dto.Amount,
            dto.Type,
            dto.Meta?.Particulars,
            dto.Meta?.Code,
            dto.Meta?.Reference,
            dto.Meta?.OtherAccount,
            dto.Merchant?.Name,
            dto.Meta?.CardSuffix);
    }
}
