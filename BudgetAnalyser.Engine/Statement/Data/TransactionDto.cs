namespace BudgetAnalyser.Engine.Statement.Data;

/// <summary>
///     A Dto to persist a single transaction from a statement.
/// </summary>
public class TransactionDto
{
    /// <summary>
    ///     Gets or sets the account code.
    /// </summary>
    public required string Account { get; init; }

    /// <summary>
    ///     Gets or sets the transaction amount, debits are negative.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    ///     Gets or sets the budget bucket code.
    /// </summary>
    public string? BudgetBucketCode { get; init; }

    /// <summary>
    ///     Gets or sets the transaction date.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    ///     Gets or sets the transaction description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     The unique identifier for the transaction.  Ideally this should not be public settable, but this is used during
    ///     serialisation.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets or sets the transaction reference1.
    /// </summary>
    public string? Reference1 { get; init; }

    /// <summary>
    ///     Gets or sets the transaction reference2.
    /// </summary>
    public string? Reference2 { get; init; }

    /// <summary>
    ///     Gets or sets the transaction reference3.
    /// </summary>
    public string? Reference3 { get; init; }

    /// <summary>
    ///     Gets or sets the type code of the transaction.
    /// </summary>
    public required string TransactionType { get; init; }
}
