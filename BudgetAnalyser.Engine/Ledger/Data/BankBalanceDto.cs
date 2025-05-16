namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for <see cref="BankBalance" />
/// </summary>
public record BankBalanceDto(string Account, decimal Balance);
