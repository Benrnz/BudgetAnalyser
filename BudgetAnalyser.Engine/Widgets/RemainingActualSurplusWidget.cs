using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Monitors the remaining surplus against the actual available surplus funds available this month from the current ledger book.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.ProgressBarWidget" />
public class RemainingActualSurplusWidget : ProgressBarWidget
{
    private readonly ILogger nullLogger = new NullLogger();
    private readonly string standardStyle;
    private IBudgetCurrencyContext? budget;
    private GlobalFilterCriteria? filter;
    private LedgerBook? ledgerBook;
    private LedgerCalculation? ledgerCalculator;
    private ILogger? logger;
    private TransactionSetModel? transactionSet;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RemainingActualSurplusWidget" /> class.
    /// </summary>
    public RemainingActualSurplusWidget()
    {
        Category = WidgetGroup.PeriodicTrackingSectionName;
        DetailedText = "Bank Surplus";
        Name = "Surplus A";
        Dependencies = [typeof(TransactionSetModel), typeof(GlobalFilterCriteria), typeof(LedgerBook), typeof(LedgerCalculation), typeof(IBudgetCurrencyContext), typeof(ILogger)];
        this.standardStyle = "WidgetStandardStyle3";
    }

    private ILogger Logger => this.logger ?? this.nullLogger;

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    public override void Update(params object[] input)
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

        this.transactionSet = input[0] as TransactionSetModel;
        this.filter = input[1] as GlobalFilterCriteria;
        this.ledgerBook = input[2] as LedgerBook;
        this.ledgerCalculator = input[3] as LedgerCalculation;
        this.budget = input[4] as IBudgetCurrencyContext;
        this.logger = input[5] as ILogger;

        if (this.ledgerBook is null
            || this.budget is null
            || this.transactionSet is null
            || this.ledgerCalculator is null
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
        Logger.LogInfo(_ => "Dependencies are valid, enabled == true");
        var openingBalance = CalculateOpeningBalance();
        Logger.LogInfo(_ => $"Opening Balance = {openingBalance:C}");
        var ledgerLine = this.ledgerCalculator.LocateApplicableLedgerLine(this.ledgerBook, this.filter.BeginDate.Value, this.filter.EndDate.Value);
        if (ledgerLine is null)
        {
            ToolTip = "No ledger entries can be found in the date range.";
            Enabled = false;
            Logger.LogInfo(_ => $"No LedgerLine was found for the date range {this.filter.BeginDate.Value:d} and {this.filter.EndDate.Value:d}");
            return;
        }

        Logger.LogInfo(_ => $"LedgerLine: {ledgerLine.Date:d} TotalBankBalance {ledgerLine.TotalBankBalance:C}");

        var remainingBalance = this.ledgerCalculator.CalculateCurrentPeriodSurplusBalance(ledgerLine, this.filter, this.transactionSet);
        Logger.LogInfo(_ => $"Remaining period surplus balance is: {remainingBalance:C}");

        Maximum = Convert.ToDouble(openingBalance);
        Value = Convert.ToDouble(remainingBalance);
        Minimum = 0;
        ColourStyleName = remainingBalance < 0.2M * openingBalance ? WidgetWarningStyle : this.standardStyle;

        ToolTip = $"Remaining Surplus for period is {remainingBalance:C} of {openingBalance:C} {remainingBalance / openingBalance:P0}";
    }

    private decimal CalculateOpeningBalance()
    {
        var line = this.ledgerCalculator!.LocateApplicableLedgerLine(this.ledgerBook!, this.filter!);
        return line?.CalculatedSurplus ?? 0;
    }
}
