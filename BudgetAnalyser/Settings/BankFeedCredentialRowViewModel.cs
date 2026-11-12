using BudgetAnalyser.Engine.BankFeed;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetAnalyser.Settings;

/// <summary>
///     One credential field on the "Bank feed connection" card. The rows are generated from the provider's credential fields, so the view never names a vendor.
///     A stored value is never read back into this row, only whether one exists.
/// </summary>
public partial class BankFeedCredentialRowViewModel(BankFeedCredentialField credentialField, bool hasStoredValue) : ObservableObject
{
    public bool HasStoredValue { get; } = hasStoredValue;

    public bool IsSecret => credentialField.IsSecret;

    public string Key => credentialField.Key;

    public string Label => credentialField.Label;

    /// <summary>
    ///     The value the user has typed, waiting to be saved. Secret fields are set from a PasswordBox by the view's code-behind.
    /// </summary>
    [ObservableProperty]
    public partial string NewValue { get; set; } = string.Empty;

    public string StoredValueText => HasStoredValue ? "Saved. Enter a new value to replace it." : "Not saved.";
}
