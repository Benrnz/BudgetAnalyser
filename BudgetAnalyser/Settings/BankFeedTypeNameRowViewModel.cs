using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetAnalyser.Settings;

/// <summary>
///     One entry in an account's transaction type names table: a bank feed type and the name to use in Budget Analyser instead.
/// </summary>
public partial class BankFeedTypeNameRowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string FeedType { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ImportAs { get; set; } = string.Empty;
}
