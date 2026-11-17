namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     The response from POST /v1/refresh. It only says whether the request was accepted.
/// </summary>
internal record AkahuRefreshResponseDto(bool Success, string? Message) : AkahuResponseDto(Success, Message);
