using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed;

namespace BudgetAnalyser.Settings;

/// <summary>
///     A bank feed field shown in the field mapping combo boxes.
/// </summary>
public record BankFeedSourceFieldOption(BankFeedSourceField Value, string DisplayName)
{
    public static IReadOnlyList<BankFeedSourceFieldOption> All { get; } =
    [
        new(BankFeedSourceField.None, "(none)"),
        new(BankFeedSourceField.Description, "Description"),
        new(BankFeedSourceField.MerchantName, "Merchant name"),
        new(BankFeedSourceField.Particulars, "Particulars"),
        new(BankFeedSourceField.Code, "Code"),
        new(BankFeedSourceField.Reference, "Reference"),
        new(BankFeedSourceField.OtherPartyAccount, "Other party account"),
        new(BankFeedSourceField.Type, "Type"),
        new(BankFeedSourceField.CardSuffix, "Card suffix")
    ];
}

/// <summary>
///     A choice in the "Import into" combo box: a Budget Analyser account, or no account meaning "Don't import".
/// </summary>
public record BankFeedAccountChoice(Account? Account)
{
    public string DisplayName => Account?.Name ?? "Don't import";
}
