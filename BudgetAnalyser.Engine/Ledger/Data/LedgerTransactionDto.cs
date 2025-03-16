namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for all subclasses of <see cref="LedgerTransaction" />.  All subclasses are flattened into this type.
/// </summary>
public class LedgerTransactionDto
{
    /// <summary>
    ///     Gets or sets the account identifier.
    /// </summary>
    public string? Account { get; init; }

    /// <summary>
    ///     Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    ///     Gets or sets the automatic matching reference.
    /// </summary>
    public string? AutoMatchingReference { get; init; }

    /// <summary>
    ///     Gets or sets the date.
    /// </summary>
    public DateOnly? Date { get; init; }

    /// <summary>
    ///     Gets or sets the transaction identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets or sets the narrative.
    /// </summary>
    public string? Narrative { get; init; }

    /// <summary>
    ///     Gets or sets the type of the transaction.
    /// </summary>
    public string? TransactionType { get; init; }
}
