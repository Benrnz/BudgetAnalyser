using System.Globalization;
using BudgetAnalyser.Engine.BankFeed;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetAnalyser.Transactions;

/// <summary>
///     One mapped account in the "Download from bank feed" dialog's checklist.
/// </summary>
public partial class BankFeedImportAccountRowViewModel(BankFeedAccountMapping mapping) : ObservableObject
{
    public string AccountName => Mapping.Account.Name;

    public string DataAsOfText => FeedAccount?.DataAsOf is { } asOf
        ? string.Format(CultureInfo.CurrentCulture, "data as of {0:g}", asOf.ToLocalTime())
        : string.Empty;

    /// <summary>
    ///     The account's details from the bank feed. Null until they are loaded, or if the feed no longer has the account.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DataAsOfText), nameof(FeedName))]
    public partial BankFeedAccount? FeedAccount { get; set; }

    public string FeedName => FeedAccount is null ? string.Empty : $"{FeedAccount.Institution}  {FeedAccount.Name}";

    [ObservableProperty]
    public partial bool IsAvailable { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSelected { get; set; } = true;

    /// <summary>
    ///     True when the feed's data for this account was last refreshed before the end of the To date, so recent transactions may be missing.
    /// </summary>
    [ObservableProperty]
    public partial bool IsStale { get; set; }

    public BankFeedAccountMapping Mapping { get; } = mapping;

    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;
}
