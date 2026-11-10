namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     A settled transaction downloaded from a bank feed, in a vendor-neutral shape. Every provider produces this shape. It is converted into a
///     <see cref="Transactions.Transaction" /> using the user's field mapping for the account.
/// </summary>
/// <param name="ExternalId">The provider's stable identifier for the transaction.</param>
/// <param name="ExternalAccountId">The provider's identifier for the account the transaction belongs to.</param>
/// <param name="Institution">The name of the bank or institution that holds the account.</param>
/// <param name="Date">The transaction date in New Zealand local time.</param>
/// <param name="Description">The transaction description as provided by the bank.</param>
/// <param name="Amount">The amount. Negative values are money leaving the account.</param>
/// <param name="TypeName">The provider's transaction type, if available.</param>
/// <param name="Particulars">The particulars field entered when the payment was made, if available.</param>
/// <param name="Code">The code field entered when the payment was made, if available.</param>
/// <param name="Reference">The reference field entered when the payment was made, if available.</param>
/// <param name="OtherPartyAccount">The formatted account number of the other party, if available.</param>
/// <param name="MerchantName">The merchant name, if the provider recognised the merchant.</param>
/// <param name="CardSuffix">The last digits of the card used, if available.</param>
public record BankFeedTransaction(
    string ExternalId,
    string ExternalAccountId,
    string Institution,
    DateOnly Date,
    string Description,
    decimal Amount,
    string? TypeName,
    string? Particulars,
    string? Code,
    string? Reference,
    string? OtherPartyAccount,
    string? MerchantName,
    string? CardSuffix);
