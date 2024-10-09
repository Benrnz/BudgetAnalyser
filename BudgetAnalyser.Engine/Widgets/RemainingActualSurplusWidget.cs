using System;
using System.Globalization;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Monitors the remaining surplus against the actual available surplus funds available this month from the current ledger book.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.ProgressBarWidget" />
public class RemainingActualSurplusWidget : ProgressBarWidget
{
    private readonly string standardStyle;
    private GlobalFilterCriteria filter;
    private LedgerBook ledgerBook;
    private LedgerCalculation ledgerCalculator;
    private StatementModel statement;
    private IBudgetCurrencyContext budget;
    private ILogger logger;
    private ILogger nullLogger = new NullLogger();

    /// <summary>
    ///     Initializes a new instance of the <see cref="RemainingActualSurplusWidget" /> class.
    /// </summary>
    public RemainingActualSurplusWidget()
    {
        Category = WidgetGroup.PeriodicTrackingSectionName;
        DetailedText = "Bank Surplus";
        Name = "Surplus A";
        Dependencies = new[] { typeof(StatementModel), typeof(GlobalFilterCriteria), typeof(LedgerBook), typeof(LedgerCalculation), typeof(IBudgetCurrencyContext), typeof(ILogger) };
        this.standardStyle = "WidgetStandardStyle3";
    }

    private ILogger Logger { get => this.logger == this.nullLogger ? null : this.logger; }

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

        this.statement = (StatementModel)input[0];
        this.filter = (GlobalFilterCriteria)input[1];
        this.ledgerBook = (LedgerBook)input[2];
        this.ledgerCalculator = (LedgerCalculation)input[3];
        this.budget = (IBudgetCurrencyContext)input[4];
        this.logger = (ILogger)input[5];

        if (this.ledgerBook is null
            || this.budget is null
            || this.statement is null
            || this.filter is null
            || this.filter.Cleared
            || this.filter.BeginDate is null
            || this.filter.EndDate is null)
        {
            Enabled = false;
            return;
        }

        if (!RemainingBudgetBucketWidget.ValidatePeriod(this.budget.Model.BudgetCycle, this.filter.BeginDate.Value, this.filter.EndDate.Value, out var validationMessage))
        {
            // Fortnightly will calculate to 1 here as well, so this is a valid test.
            ToolTip = validationMessage;
            Enabled = false;
            return;
        }

        Enabled = true;
        Logger.LogInfo(l => "Dependencies are valid, enabled == true");
        var openingBalance = CalculateOpeningBalance();
        Logger.LogInfo(l => l.Format("Opening Balance = {0:C}", openingBalance));
        var ledgerLine = this.ledgerCalculator.LocateApplicableLedgerLine(this.ledgerBook, this.filter.BeginDate.Value, this.filter.EndDate.Value);
        if (ledgerLine is null)
        {
            ToolTip = "No ledger entries can be found in the date range.";
            Enabled = false;
            Logger.LogInfo(l => l.Format("No LedgerLine was found for the date range {0:d} and {1:d}", this.filter.BeginDate.Value, this.filter.EndDate.Value));
            return;
        }

        Logger.LogInfo(l => l.Format("LedgerLine: {0:d} TotalBankBalance {1:C}", ledgerLine.Date, ledgerLine.TotalBankBalance));

        var remainingBalance = this.ledgerCalculator.CalculateCurrentPeriodSurplusBalance(ledgerLine, this.filter, this.statement);
        Logger.LogInfo(l => l.Format("Remaining period surplus balance is: {0:C}", remainingBalance));

        Maximum = Convert.ToDouble(openingBalance);
        Value = Convert.ToDouble(remainingBalance);
        Minimum = 0;
        if (remainingBalance < 0.2M * openingBalance)
        {
            ColourStyleName = WidgetWarningStyle;
        }
        else
        {
            ColourStyleName = this.standardStyle;
        }

        ToolTip = $"Remaining Surplus for period is {remainingBalance:C} of {openingBalance:C} {remainingBalance/openingBalance:P0}";
    }

    private decimal CalculateOpeningBalance()
    {
        var line = this.ledgerCalculator.LocateApplicableLedgerLine(this.ledgerBook, this.filter);
        return line?.CalculatedSurplus ?? 0;
    }
}