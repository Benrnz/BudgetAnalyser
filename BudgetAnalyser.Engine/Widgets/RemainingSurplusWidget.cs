using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A widget to monitor the remaining budgeted surplus as compared to the current budget surplus figure. (Not the
///     actual surplus from the ledger book).
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.RemainingBudgetBucketWidget" />
public class RemainingSurplusWidget : RemainingBudgetBucketWidget
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RemainingSurplusWidget" /> class.
    /// </summary>
    public RemainingSurplusWidget()
    {
        DetailedText = "Budgeted Surplus";
        Name = "Surplus B";
        DependencyMissingToolTip = "A Statement, Budget, or a Filter are not present, surplus cannot be calculated.";
        RemainingBudgetToolTip = "Remaining Surplus for period is {0:C} {1:P0}";
        BucketCode = SurplusBucket.SurplusCode;
    }

    protected override decimal CalculateTotalSpendInPeriod(LedgerEntryLine line)
    {
        var spend = base.CalculateTotalSpendInPeriod(line);
        var overspentLedgers = LedgerCalculation!.CalculateOverSpentLedgers(Statement!, line, Filter!.BeginDate!.Value, Filter!.EndDate!.Value);
        var overdrawnBalance = overspentLedgers.Sum(t => t.Amount);
        return spend + overdrawnBalance;
    }

    /// <summary>
    ///     Monthlies the budget amount.
    /// </summary>
    protected override decimal LedgerBucketBalanceOrBudgetAmount()
    {
        return Budget is null ? 0 : Budget.Model.Surplus;
    }
}
