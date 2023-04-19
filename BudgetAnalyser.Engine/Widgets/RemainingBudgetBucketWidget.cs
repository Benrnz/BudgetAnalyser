using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Use this widget base class for widgets that monitor budget bucket spend.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.ProgressBarWidget" />
public abstract class RemainingBudgetBucketWidget : ProgressBarWidget
{
    private readonly string standardStyle;
    private IBudgetBucketRepository bucketRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RemainingBudgetBucketWidget" /> class.
    /// </summary>
    protected RemainingBudgetBucketWidget()
    {
        Category = WidgetGroup.PeriodicTrackingSectionName;
        Dependencies = new[]
        {
            typeof(IBudgetCurrencyContext),
            typeof(StatementModel),
            typeof(GlobalFilterCriteria),
            typeof(IBudgetBucketRepository),
            typeof(LedgerBook),
            typeof(LedgerCalculation)
        };
        RecommendedTimeIntervalUpdate = TimeSpan.FromHours(6);
        RemainingBudgetToolTip = "Remaining Balance for period is {0:C}";
        this.standardStyle = "WidgetStandardStyle3";
        BucketCode = "<NOT SET>";
    }

    /// <summary>
    ///     Gets or sets the bucket code.
    /// </summary>
    public string BucketCode { get; set; }

    /// <summary>
    ///     Gets the budget model.
    /// </summary>
    protected IBudgetCurrencyContext Budget { get; private set; }

    /// <summary>
    ///     Gets or sets the dependency missing tool tip.
    /// </summary>
    protected string DependencyMissingToolTip { get; set; }

    /// <summary>
    ///     Gets the global filter.
    /// </summary>
    private GlobalFilterCriteria Filter { get; set; }

    /// <summary>
    ///     Gets the ledger book model.
    /// </summary>
    private LedgerBook LedgerBook { get; set; }

    /// <summary>
    ///     Gets the ledger calculator.
    /// </summary>
    private LedgerCalculation LedgerCalculation { get; set; }

    /// <summary>
    ///     Gets or sets the remaining budget tool tip.
    /// </summary>
    protected string RemainingBudgetToolTip { get; set; }

    /// <summary>
    ///     Gets the statement model.
    /// </summary>
    private StatementModel Statement { get; set; }

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    public override void Update([NotNull] params object[] input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (!ValidateUpdateInput(input))
        {
            Enabled = false;
            return;
        }

        Budget = (IBudgetCurrencyContext)input[0];
        Statement = (StatementModel)input[1];
        Filter = (GlobalFilterCriteria)input[2];
        this.bucketRepository = (IBudgetBucketRepository)input[3];
        LedgerBook = (LedgerBook)input[4];
        LedgerCalculation = (LedgerCalculation)input[5];

        if (Statement == null
            || Budget == null
            || Filter == null
            || Filter.Cleared
            || Filter.BeginDate == null
            || Filter.EndDate == null
            || LedgerCalculation == null
            || LedgerBook == null
            || this.bucketRepository == null)
        {
            Enabled = false;
            return;
        }

        if (!this.bucketRepository.IsValidCode(BucketCode))
        {
            Enabled = false;
            return;
        }

        if (Budget.Model.BudgetCycle == BudgetCycle.Monthly)
        {
            if (Filter.BeginDate.Value.DurationInMonths(Filter.EndDate.Value) > 1)
            {
                Enabled = false;
                ToolTip = DesignedForOneMonthOnly;
                return;
            }
        }

        if (Budget.Model.BudgetCycle == BudgetCycle.Fortnightly)
        {
            if (Filter.EndDate.Value.Subtract(Filter.BeginDate.Value).Days > 14)
            {
                Enabled = false;
                ToolTip = DesignedForOneFortnightOnly;
                return;
            }
        }

        Enabled = true;
        var totalBudget = LedgerBucketBalanceOrBudgetAmount();
        Maximum = Convert.ToDouble(totalBudget);

        var ledgerLine = LedgerCalculation.LocateApplicableLedgerLine(LedgerBook, Filter);
        var totalSpend = LedgerCalculation.CalculateCurrentPeriodBucketSpend(ledgerLine, Filter, Statement, BucketCode);

        var remainingBalance = totalBudget + totalSpend;
        if (remainingBalance < 0)
        {
            remainingBalance = 0;
        }

        Value = Convert.ToDouble(remainingBalance);
        ToolTip = string.Format(CultureInfo.CurrentCulture, RemainingBudgetToolTip, remainingBalance, remainingBalance/totalBudget);

        if (remainingBalance < 0.2M * totalBudget)
        {
            ColourStyleName = WidgetWarningStyle;
        }
        else
        {
            ColourStyleName = this.standardStyle;
        }
    }

    /// <summary>
    ///     Retrieves the current ledger balance for the bucket if there is one, or the budget amount.
    /// </summary>
    protected virtual decimal LedgerBucketBalanceOrBudgetAmount()
    {
        Debug.Assert(Filter.BeginDate != null);
        Debug.Assert(Filter.EndDate != null);

        var ledgerLine = LedgerCalculation.LocateApplicableLedgerLine(LedgerBook, Filter);

        if (LedgerBook == null || ledgerLine == null || ledgerLine.Entries.All(e => e.LedgerBucket.BudgetBucket.Code != BucketCode))
        {
            return Budget.Model.Expenses.Single(b => b.Bucket.Code == BucketCode).Amount;
        }

        return ledgerLine.Entries.First(e => e.LedgerBucket.BudgetBucket.Code == BucketCode).Balance;
    }
}