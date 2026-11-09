namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     Optional data Akahu extracted from a transaction. Every field may be missing.
/// </summary>
/// <param name="Particulars">The particulars field entered when the payment was made.</param>
/// <param name="Code">The code field entered when the payment was made.</param>
/// <param name="Reference">The reference field entered when the payment was made.</param>
/// <param name="OtherAccount">The formatted account number of the other party.</param>
/// <param name="Conversion">Currency conversion details, if the transaction was in another currency.</param>
/// <param name="CardSuffix">The last digits of the card used.</param>
internal record AkahuTransactionMetaDto(string? Particulars, string? Code, string? Reference, string? OtherAccount, AkahuConversionDto? Conversion, string? CardSuffix);
