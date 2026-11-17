namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Describes one credential value a <see cref="IBankFeedProvider" /> needs the user to supply.
/// </summary>
/// <param name="Key">The key used to store and retrieve the value in the <see cref="IBankFeedCredentialStore" />.</param>
/// <param name="Label">The label to show the user.</param>
/// <param name="IsSecret">True if the value is secret and must be masked in the UI and never logged.</param>
public record BankFeedCredentialField(string Key, string Label, bool IsSecret);
