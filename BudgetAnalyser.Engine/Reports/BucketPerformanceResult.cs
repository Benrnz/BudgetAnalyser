using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Reports;

/// <summary>
///     A Data Transfer Object to contain the output of a bucket spending analysis.
/// </summary>
public class BucketPerformanceResult
{
    /// <summary>
    ///     Gets the calculated average spend.
    /// </summary>
    public decimal AverageSpend { get; internal set; }

    /// <summary>
    ///     Gets the calculated balance.
    /// </summary>
    public decimal Balance { get; internal init; }

    /// <summary>
    ///     Gets the bucket.
    /// </summary>
    public required BudgetBucket Bucket { get; set; }

    /// <summary>
    ///     Gets the budget amount.
    /// </summary>
    public decimal Budget { get; internal init; }

    /// <summary>
    ///     Gets the calculated budget compared to average.
    /// </summary>
    public required string BudgetComparedToAverage { get; set; }

    /// <summary>
    ///     Gets the calculated budget total.
    /// </summary>
    public decimal BudgetTotal { get; internal init; }

    /// <summary>
    ///     Gets the calculated percentage.
    /// </summary>
    public double Percent => BudgetTotal < 0.01M ? (double)Math.Round(TotalSpent / 0.01M, 2) : (double)Math.Round(TotalSpent / BudgetTotal, 2);

    /// <summary>
    ///     Gets the summary.
    /// </summary>
    public string Summary =>
        Percent > 1
            ? $"{Percent - 1:P0} ({Balance:C}) OVER Budget of {BudgetTotal:C}.  Total Spent: {TotalSpent:C}.  Single Month Budget: {Budget:C}"
            : $"{Percent:P0} ({Balance:C}) under Budget of {BudgetTotal:C}.  Total Spent: {TotalSpent:C}.  Single Month Budget: {Budget:C}";

    /// <summary>
    ///     Gets the calculated total spent.
    /// </summary>
    public decimal TotalSpent { get; internal set; }
}
