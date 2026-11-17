namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     The response from GET /v1/me.
/// </summary>
internal record AkahuMeResponseDto(bool Success, AkahuMeDto? Item, string? Message) : AkahuResponseDto(Success, Message);
