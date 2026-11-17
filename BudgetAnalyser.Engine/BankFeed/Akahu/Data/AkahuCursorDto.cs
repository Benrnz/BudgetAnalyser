namespace BudgetAnalyser.Engine.BankFeed.Akahu.Data;

/// <summary>
///     The paging cursor. <paramref name="Next" /> is null on the last page, and must never be sent back to the API when null.
/// </summary>
internal record AkahuCursorDto(string? Next);
