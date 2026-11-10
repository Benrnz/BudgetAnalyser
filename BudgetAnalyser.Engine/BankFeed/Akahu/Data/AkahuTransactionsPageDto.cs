namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     One page of the response from GET /v1/accounts/{id}/transactions. When <paramref name="Cursor" /> has a non-null Next value, more pages are available.
/// </summary>
internal record AkahuTransactionsPageDto(bool Success, AkahuTransactionDto[]? Items, AkahuCursorDto? Cursor, string? Message) : AkahuResponseDto(Success, Message);
