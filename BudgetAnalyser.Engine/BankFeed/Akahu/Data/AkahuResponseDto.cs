namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     The standard response wrapper used by every Akahu API response. If <paramref name="Success" /> is false, <paramref name="Message" /> describes the problem.
/// </summary>
internal abstract record AkahuResponseDto(bool Success, string? Message);
