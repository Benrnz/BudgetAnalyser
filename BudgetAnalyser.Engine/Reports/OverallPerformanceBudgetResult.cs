namespace BudgetAnalyser.Engine.Reports;

/// <summary>
///     A class to store the analysis result data for the Overall Performance chart.
/// </summary>
public class OverallPerformanceBudgetResult
{
    /// <summary>
    ///     Gets the analysis list.
    /// </summary>
    public IEnumerable<BucketPerformanceResult> Analyses => AnalysesList;

    internal IList<BucketPerformanceResult> AnalysesList { get; set; } = new List<BucketPerformanceResult>();

    /// <summary>
    ///     Gets the average spend per month based on statement transaction data over a period of time.
    ///     This excludes Surplus transactions, these are budgeted expenses only.
    ///     Expected to be negative.
    /// </summary>
    public decimal AverageSpend { get; internal set; }

    /// <summary>
    ///     Gets the average surplus spending per month based on statement transaction data over a period of time.
    /// </summary>
    public decimal AverageSurplus { get; internal set; }

    /// <summary>
    ///     Gets the calculated duration in months.
    /// </summary>
    public int DurationInPeriods { get; internal set; }

    /// <summary>
    ///     Indicates that the report did not run due to errors. See <see cref="ValidationMessage" /> for details.
    /// </summary>
    public bool Error { get; internal set; }

    /// <summary>
    ///     Indicates the presence of a message that should be surfaced to the user.
    /// </summary>
    public bool HasValidationMessage => ValidationMessage != string.Empty || UsesMultipleBudgets;

    /// <summary>
    ///     Gets the calculated overall performance rating.
    /// </summary>
    public decimal OverallPerformance { get; internal set; }

    /// <summary>
    ///     Gets the calculated total budget expenses.
    /// </summary>
    public decimal TotalBudgetExpenses { get; internal set; }

    /// <summary>
    ///     Gets a calculated value indicating whether this analysis spans multiple budgets.
    /// </summary>
    public bool UsesMultipleBudgets { get; internal set; }

    /// <summary>
    ///     Validation messages based on inputs that may not produce the expected results.
    ///     For example, if it covers multiple budgets during the period specified.
    ///     Or, if, it's multiple budgets and they have different periods, fortnightly and monthly.
    /// </summary>
    public string ValidationMessage { get; internal set; } = string.Empty;
}
