﻿using System;
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
        RemainingBudgetToolTip = "Remaining Balance for period is {0:C} {1:P0}";
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
        if (input is null)
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

        if (Statement is null
            || Budget is null
            || Filter is null
            || Filter.Cleared
            || Filter.BeginDate is null
            || Filter.EndDate is null
            || LedgerCalculation is null
            || LedgerBook is null
            || this.bucketRepository is null)
        {
            Enabled = false;
            return;
        }

        if (!this.bucketRepository.IsValidCode(BucketCode))
        {
            Enabled = false;
            return;
        }

        if (!ValidatePeriod(Budget.Model.BudgetCycle, Filter.BeginDate.Value, Filter.EndDate.Value, out var validationMessage))
        {
            ToolTip = validationMessage;
            Enabled = false;
            return;
        }

        Enabled = true;
        var totalBudget = LedgerBucketBalanceOrBudgetAmount();
        Maximum = Convert.ToDouble(totalBudget);

        var ledgerLine = LedgerCalculation.LocateApplicableLedgerLine(LedgerBook, Filter);
        if (ledgerLine is null)
        {
            Enabled = false;
            return;
        }

        var totalSpend = LedgerCalculation.CalculateCurrentPeriodBucketSpend(ledgerLine, Filter, Statement, BucketCode);

        var remainingBalance = totalBudget + totalSpend;
        if (remainingBalance < 0)
        {
            remainingBalance = 0;
        }

        Value = Convert.ToDouble(remainingBalance);
        ToolTip = string.Format(CultureInfo.CurrentCulture, RemainingBudgetToolTip, remainingBalance, remainingBalance / totalBudget);

        ColourStyleName = remainingBalance < 0.2M * totalBudget ? WidgetWarningStyle : this.standardStyle;
    }

    /// <summary>
    ///     Retrieves the current ledger balance for the bucket if there is one, or the budget amount.
    /// </summary>
    protected virtual decimal LedgerBucketBalanceOrBudgetAmount()
    {
        Debug.Assert(Filter.BeginDate is not null);
        Debug.Assert(Filter.EndDate is not null);

        var ledgerLine = LedgerCalculation.LocateApplicableLedgerLine(LedgerBook, Filter);

        return LedgerBook is null || ledgerLine is null || ledgerLine.Entries.All(e => e.LedgerBucket.BudgetBucket.Code != BucketCode)
            ? Budget.Model.Expenses.Single(b => b.Bucket.Code == BucketCode).Amount
            : ledgerLine.Entries.First(e => e.LedgerBucket.BudgetBucket.Code == BucketCode).Balance;
    }

    internal static bool ValidatePeriod(BudgetCycle budgetCycle, DateTime inclBeginDate, DateTime inclEndDate, out string validationMessage)
    {
        if (budgetCycle == BudgetCycle.Monthly)
        {
            if (inclBeginDate.DurationInMonths(inclEndDate) > 1)
            {
                validationMessage = DesignedForOneMonthOnly;
                return false;
            }
        }

        if (budgetCycle == BudgetCycle.Fortnightly)
        {
            if (inclEndDate.Subtract(inclBeginDate).Days > 14)
            {
                validationMessage = DesignedForOneFortnightOnly;
                return false;
            }
        }

        validationMessage = string.Empty;
        return true;
    }
}
