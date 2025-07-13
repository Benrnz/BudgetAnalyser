using System.Globalization;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A widget to show the number of overspent buckets for the month. Compares actual spent transactions against a ledger in the ledger book, if there is one, or the current Budget if there isn't.
///     The budget used is the currently selected budget from the <see cref="BudgetCurrencyContext" /> instance given.  It may not be the current one as compared to today's date.
/// </summary>
public class OverspentWarning : Widget
{
    private readonly ILogger logger;

    private decimal tolerance;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OverspentWarning" /> class.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global // Instantiated by the Widget Service / Repo.
    public OverspentWarning()
    {
        Category = WidgetGroup.PeriodicTrackingSectionName;
        this.logger = new NullLogger();
        Dependencies = [typeof(TransactionSetModel), typeof(IBudgetCurrencyContext), typeof(GlobalFilterCriteria), typeof(LedgerBook), typeof(LedgerCalculation)];
        DetailedText = "Overspent";
        ImageResourceName = null;
        RecommendedTimeIntervalUpdate = TimeSpan.FromMinutes(15);
        this.tolerance = 25; // By default, must be overspent by 10 dollars to be considered overspent.
    }

    public OverspentWarning(ILogger logger) : this()
    {
        // Only used in unit tests currently.
        this.logger = logger;
    }

    internal IEnumerable<KeyValuePair<BudgetBucket, decimal>> OverSpentSummary { get; private set; } = Array.Empty<KeyValuePair<BudgetBucket, decimal>>();

    public decimal Tolerance
    {
        get => this.tolerance;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Tolerance must be greater than or equal to 0.");
            }

            this.tolerance = value;
        }
    }

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    public override void Update(params object[] input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        this.logger.LogInfo(_ => "OverspentWarning Widget Update called.");
        if (!ValidateUpdateInput(input))
        {
            Enabled = false;
            return;
        }

        if (input[1] is not IBudgetCurrencyContext budget
            || input[3] is not LedgerBook ledgerBook
            || input[0] is not TransactionSetModel statement
            || input[4] is not LedgerCalculation ledgerCalculator
            || input[2] is not GlobalFilterCriteria filter
            || filter.Cleared
            || filter.BeginDate is null
            || filter.EndDate is null)
        {
            this.logger.LogInfo(_ => "Statement, budget, ledger book, or ledger calculator are null. Or date filter is invalid.");
            Enabled = false;
            ToolTip = "LedgerBook, Statement, or Filter are not set/loaded.";
            return;
        }

        if (!RemainingBudgetBucketWidget.ValidatePeriod(budget.Model.BudgetCycle, filter.BeginDate.Value, filter.EndDate.Value, out var validationMessage))
        {
            this.logger.LogInfo(_ => $"Difference in months between begin and end != 1 month. BeginDate: {filter.BeginDate.Value}, EndDate: {filter.EndDate.Value}");
            Enabled = false;
            ToolTip = validationMessage;
            return;
        }

        var ledgerLine = ledgerCalculator.LocateApplicableLedgerLine(ledgerBook, filter.BeginDate.Value, filter.EndDate.Value);
        if (ledgerLine is null)
        {
            Enabled = false;
            return;
        }

        Enabled = true;
        this.logger.LogInfo(l => l.Format("Using this LedgerEntryLine: {0}", ledgerLine.Date));
        var currentLedgerBalances = ledgerCalculator.CalculateCurrentPeriodLedgerBalances(ledgerLine, filter, statement);
        this.logger.LogInfo(l =>
        {
            var builder = new StringBuilder();
            currentLedgerBalances.Select(ledger => l.Format("{0} {1}", ledger.Key, ledger.Value))
                .ToList()
                .ForEach(x => builder.AppendLine(x));

            return builder.ToString();
        });
        var warnings = currentLedgerBalances.Count(balance => balance.Value < -this.tolerance);
        var logWarning = warnings;
        this.logger.LogInfo(l => l.Format("{0} overspent ledgers within tolerance {1} detected.", logWarning, this.tolerance));
        // Check other budget buckets that are not represented in the ledger book.
        warnings += SearchForOtherNonLedgerBookOverspentBuckets(statement, filter.BeginDate.Value, filter.EndDate.Value, budget, currentLedgerBalances);

        if (warnings > 0)
        {
            this.logger.LogInfo(l => l.Format("{0} total warnings found, updating UI.", warnings));
            LargeNumber = warnings.ToString(CultureInfo.CurrentCulture);
            var builder = new StringBuilder();
            OverSpentSummary = currentLedgerBalances.Where(kvp => kvp.Value < -this.tolerance).OrderBy(kvp => kvp.Key);
            foreach (var ledger in OverSpentSummary)
            {
                builder.AppendFormat(CultureInfo.CurrentCulture, "{0} is overspent by {1:C}", ledger.Key, ledger.Value);
                builder.AppendLine();
            }

            ToolTip = builder.ToString();
            ColourStyleName = WidgetWarningStyle;
        }
        else
        {
            this.logger.LogInfo(_ => "No warnings found... updating UI");
            LargeNumber = "0";
            ToolTip = "No overspent ledgers for the period beginning " + filter.BeginDate.Value.ToString("d", CultureInfo.CurrentCulture);
            ColourStyleName = WidgetStandardStyle;
        }
    }

    private int SearchForOtherNonLedgerBookOverspentBuckets(
        TransactionSetModel transactionSet,
        DateOnly inclBeginDate,
        DateOnly inclEndDate,
        IBudgetCurrencyContext budget,
        IDictionary<BudgetBucket, decimal> currentLedgerBalances)
    {
        var warnings = 0;
        var transactions = transactionSet.Transactions.Where(t => t.Date >= inclBeginDate && t.Date <= inclEndDate).ToList();
        this.logger.LogInfo(l => l.Format("SearchForOtherNonLedgerBookOverSpentBuckets: {0} statement transactions found.", transactions.Count()));
        foreach (var expense in budget.Model.Expenses.Where(e => e.Bucket is ExpenseBucket))
        {
            if (currentLedgerBalances.ContainsKey(expense.Bucket!))
            {
                this.logger.LogInfo(l => l.Format("Ignoring {0}, exists within LedgerBook", expense.BucketCode));
                continue;
            }

            var bucketBalance = expense.Amount + transactions.Where(t => t.BudgetBucket == expense.Bucket).Sum(t => t.Amount);
            currentLedgerBalances.Add(expense.Bucket!, bucketBalance);
            this.logger.LogInfo(l => l.Format("Found non-LedgerBook Bucket: {0} Bucket calc'd balance {1}", expense.BucketCode, bucketBalance));
            if (bucketBalance < -this.tolerance)
            {
                this.logger.LogInfo(_ => "Bucket balance less than tolerance... Adding warning.");
                warnings++;
            }
        }

        this.logger.LogInfo(l => l.Format("{0} warnings found for non-LedgerBook buckets.", warnings));
        return warnings;
    }
}
