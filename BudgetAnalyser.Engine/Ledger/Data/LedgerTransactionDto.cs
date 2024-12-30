using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for all subclasses of <see cref="LedgerTransaction" />.  All subclasses are flattened into this type.
/// </summary>
public class LedgerTransactionDto
{
    /// <summary>
    ///     Gets or sets the account identifier.
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    ///     Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets the automatic matching reference.
    /// </summary>
    [UsedImplicitly]
    public string AutoMatchingReference { get; set; }

    /// <summary>
    ///     Gets or sets the date.
    /// </summary>
    [UsedImplicitly]
    public DateTime? Date { get; set; }

    /// <summary>
    ///     Gets or sets the transaction identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the narrative.
    /// </summary>
    public string Narrative { get; set; }

    /// <summary>
    ///     Gets or sets the type of the transaction.
    /// </summary>
    public string TransactionType { get; set; }
}
