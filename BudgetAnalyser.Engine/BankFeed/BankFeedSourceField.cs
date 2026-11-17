using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     A field of a <see cref="BankFeedTransaction" /> that can be used to fill a field of a Budget Analyser transaction. The user chooses these per account in a
///     <see cref="BankFeedFieldMapping" />. These names are saved in the Budget Analyser file, so existing names must never be renamed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<BankFeedSourceField>))]
public enum BankFeedSourceField
{
    /// <summary>
    ///     No field, the target is left empty.
    /// </summary>
    None,

    /// <summary>
    ///     <see cref="BankFeedTransaction.Description" />.
    /// </summary>
    Description,

    /// <summary>
    ///     <see cref="BankFeedTransaction.MerchantName" />.
    /// </summary>
    MerchantName,

    /// <summary>
    ///     <see cref="BankFeedTransaction.Particulars" />.
    /// </summary>
    Particulars,

    /// <summary>
    ///     <see cref="BankFeedTransaction.Code" />.
    /// </summary>
    Code,

    /// <summary>
    ///     <see cref="BankFeedTransaction.Reference" />.
    /// </summary>
    Reference,

    /// <summary>
    ///     <see cref="BankFeedTransaction.OtherPartyAccount" />.
    /// </summary>
    OtherPartyAccount,

    /// <summary>
    ///     <see cref="BankFeedTransaction.TypeName" />.
    /// </summary>
    Type,

    /// <summary>
    ///     <see cref="BankFeedTransaction.CardSuffix" />.
    /// </summary>
    CardSuffix
}
