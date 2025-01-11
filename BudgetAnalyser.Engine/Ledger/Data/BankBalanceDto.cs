namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for <see cref="BankBalance" />
/// </summary>
public class BankBalanceDto
{
    /// <summary>
    ///     Gets or sets the account.
    /// </summary>
    public required string Account { get; init; }

    /// <summary>
    ///     Gets or sets the balance.
    /// </summary>
    public decimal Balance { get; init; }
}
