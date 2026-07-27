namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for all subclasses of <see cref="LedgerTransaction" />.  All subclasses are flattened into this type.
/// </summary>
public record LedgerTransactionDto(
    string? Account,
    decimal Amount,
    string? AutoMatchingReference,
    DateOnly? Date,
    Guid Id,
    string? Narrative,
    string? TransactionType);
