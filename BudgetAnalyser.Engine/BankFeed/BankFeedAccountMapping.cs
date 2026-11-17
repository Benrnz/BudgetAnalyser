using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Links an account in the bank feed to a Budget Analyser account, and says how its transactions are converted.
/// </summary>
/// <param name="ExternalAccountId">The provider's opaque identifier for the account, <see cref="BankFeedAccount.ExternalId" />.</param>
/// <param name="Account">The Budget Analyser account the transactions are imported into.</param>
/// <param name="FieldMapping">How the bank feed fields fill the Budget Analyser transaction fields.</param>
public record BankFeedAccountMapping(string ExternalAccountId, Account Account, BankFeedFieldMapping FieldMapping);
