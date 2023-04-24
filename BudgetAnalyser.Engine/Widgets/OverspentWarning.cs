using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A widget to show the number of overspent buckets for the month. Compares actual spent transactions against a ledger
///     in the ledgerbook, if there is one, or the current Budget if there isn't.
///     The budget used is the currently selected budget from the <see cref="BudgetCurrencyContext" /> instance given.  It
///     may not be the current one as compared to today's date.
/// </summary>
public class OverspentWarning : Widget
{
    private readonly ILogger logger;
    private decimal tolerance;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OverspentWarning" /> class.
    /// </summary>
    public OverspentWarning()
    {
        Category = WidgetGroup.PeriodicTrackingSectionName;
        this.logger = new NullLogger();
        Dependencies = new[]
        {
            typeof(StatementModel), typeof(IBudgetCurrencyContext), typeof(GlobalFilterCriteria), typeof(LedgerBook), typeof(LedgerCalculation)
        };
        DetailedText = "Overspent";
        ImageResourceName = null;
        RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
        Tolerance = 10; // By default must be overspent by 10 dollars to be considered overspent.
    }

    public OverspentWarning(ILogger logger) : this()
    {
        this.logger = logger;
    }

    internal IEnumerable<KeyValuePair<BudgetBucket, decimal>> OverSpentSummary { get; private set; }

    /// <summary>
    ///     Gets or sets the tolerance dollar value.
    ///     By default must be overspent by 10 dollars to be considered overspent.
    /// </summary>
    public decimal Tolerance
    {
        get => this.tolerance;
        set => this.tolerance = Math.Abs(value);
    }

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    public override void Update([NotNull] params object[] input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        this.logger.LogInfo(_ => "OverspentWarning Widget Update called.");
        if (!ValidateUpdateInput(input))
        {
            Enabled = false;
            return;
        }

        var statement = (StatementModel)input[0];
        var budget = (IBudgetCurrencyContext)input[1];
        var filter = (GlobalFilterCriteria)input[2];
        var ledgerBook = (LedgerBook)input[3];
        var ledgerCalculator = (LedgerCalculation)input[4];

        if (budget == null
            || ledgerBook == null
            || statement == null
            || filter == null
            || filter.Cleared
            || filter.BeginDate == null
            || filter.EndDate == null)
        {
            this.logger.LogInfo(_ => "Statement, budget, ledgerbook, or ledgercalculator are null. Or date filter is invalid.");
            Enabled = false;
            ToolTip = "LedgerBook, Statement, or Filter are not set/loaded.";
            return;
        }

        if (!RemainingBudgetBucketWidget.ValidatePeriod(budget.Model.BudgetCycle, filter.BeginDate.Value, filter.EndDate.Value, out var validationMessage))
        {
            this.logger.LogInfo(l =>
                                    l.Format("Difference in months between begin and end != 1 month. BeginDate: {0}, EndDate: {1}", filter.BeginDate.Value, filter.EndDate.Value));
            Enabled = false;
            ToolTip = validationMessage;
            return;
        }

        Enabled = true;
        var ledgerLine = ledgerCalculator.LocateApplicableLedgerLine(ledgerBook, filter.BeginDate.Value, filter.EndDate.Value);
        this.logger.LogInfo(l => l.Format("Using this LedgerEntryLine: {0}", ledgerLine?.Date));
        IDictionary<BudgetBucket, decimal> currentLedgerBalances = ledgerCalculator.CalculateCurrentPeriodLedgerBalances(ledgerLine, filter, statement);
        this.logger.LogInfo(l =>
        {
            var builder = new StringBuilder();
            currentLedgerBalances.Select(ledger => l.Format("{0} {1}", ledger.Key, ledger.Value))
                .ToList()
                .ForEach(x => builder.AppendLine(x));

            return builder.ToString();
        });
        var warnings = currentLedgerBalances.Count(balance => balance.Value < -Tolerance);
        var logWarning = warnings;
        this.logger.LogInfo(l => l.Format("{0} overspent ledgers within tolerance {1} detected.", logWarning, Tolerance));
        // Check other budget buckets that are not represented in the ledger book.
        warnings += SearchForOtherNonLedgerBookOverspentBuckets(statement, filter.BeginDate.Value, filter.EndDate.Value, budget, currentLedgerBalances);

        if (warnings > 0)
        {
            this.logger.LogInfo(l => l.Format("{0} total warnings found, updating UI.", warnings));
            LargeNumber = warnings.ToString(CultureInfo.CurrentCulture);
            var builder = new StringBuilder();
            OverSpentSummary = currentLedgerBalances.Where(kvp => kvp.Value < -Tolerance).OrderBy(kvp => kvp.Key);
            foreach (KeyValuePair<BudgetBucket, decimal> ledger in OverSpentSummary)
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
        StatementModel statement,
        DateTime inclBeginDate,
        DateTime inclEndDate,
        IBudgetCurrencyContext budget,
        IDictionary<BudgetBucket, decimal> currentLedgerBalances)
    {
        var warnings = 0;
        List<Transaction> transactions = statement.Transactions.Where(t => t.Date >= inclBeginDate && t.Date <= inclEndDate).ToList();
        this.logger.LogInfo(l => l.Format("SearchForOtherNonLedgerBookOverSpentBuckets: {0} statement transactions found.", transactions.Count()));
        foreach (var expense in budget.Model.Expenses.Where(e => e.Bucket is BillToPayExpenseBucket))
        {
            if (currentLedgerBalances.ContainsKey(expense.Bucket))
            {
                this.logger.LogInfo(l => l.Format("Ignoring {0}, exists within LedgerBook", expense.Bucket.Code));
                continue;
            }

            var bucketBalance = expense.Amount + transactions.Where(t => t.BudgetBucket == expense.Bucket).Sum(t => t.Amount);
            currentLedgerBalances.Add(expense.Bucket, bucketBalance);
            this.logger.LogInfo(l => l.Format("Found non-LedgerBook Bucket: {0} Bucket calc'd balance {1}", expense.Bucket.Code, bucketBalance));
            if (bucketBalance < -Tolerance)
            {
                this.logger.LogInfo(_ => "Bucket balance less than tolerance... Adding warning.");
                warnings++;
            }
        }

        this.logger.LogInfo(l => l.Format("{0} warnings found for non-LedgerBook buckets.", warnings));
        return warnings;
    }
}