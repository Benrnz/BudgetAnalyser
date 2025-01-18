// ReSharper disable UnusedAutoPropertyAccessor.Global // Used implicitly by unit tests as well as debugging

namespace BudgetAnalyser.Engine.Mobile;

/// <summary>
///     A DTO type to export information about one ledger bucket
/// </summary>
public class SummarisedLedgerBucket
{
    /// <summary>
    ///     The name of the account as it appears on screen.
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    ///     The bucket code for the ledger
    /// </summary>
    public required string BucketCode { get; init; }

    /// <summary>
    ///     The type (behaviour) of the bucket. IE: Spent Monthly or Saved.
    /// </summary>
    public required string BucketType { get; init; }

    /// <summary>
    ///     The bucket description from the budget
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     The monthly budget amount credited into this ledger
    /// </summary>
    public decimal MonthlyBudgetAmount { get; init; }

    /// <summary>
    ///     The opening balance at the begining of the month
    /// </summary>
    public decimal OpeningBalance { get; init; }

    /// <summary>
    ///     The funds remaining in the bucket
    /// </summary>
    public decimal RemainingBalance { get; init; }
}
