namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     The response from GET /v1/accounts.
/// </summary>
internal record AkahuAccountsResponseDto(bool Success, AkahuAccountDto[]? Items, string? Message) : AkahuResponseDto(Success, Message);
