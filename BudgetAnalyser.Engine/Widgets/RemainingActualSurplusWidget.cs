using System;
using System.Globalization;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Monitors the remaining surplus against the actual available surplus funds available this month from the current
    ///     ledger book.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.ProgressBarWidget" />
    public class RemainingActualSurplusWidget : ProgressBarWidget
    {
        private readonly string standardStyle;
        private GlobalFilterCriteria filter;
        private LedgerBook ledgerBook;
        private LedgerCalculation ledgerCalculator;
        private StatementModel statement;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RemainingActualSurplusWidget" /> class.
        /// </summary>
        public RemainingActualSurplusWidget()
        {
            Category = WidgetGroup.MonthlyTrackingSectionName;
            DetailedText = "Bank Surplus";
            Name = "Surplus A";
            Dependencies = new[]
            { typeof(StatementModel), typeof(GlobalFilterCriteria), typeof(LedgerBook), typeof(LedgerCalculation) };
            this.standardStyle = "WidgetStandardStyle3";
        }

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
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

            this.statement = (StatementModel) input[0];
            this.filter = (GlobalFilterCriteria) input[1];
            this.ledgerBook = (LedgerBook) input[2];
            this.ledgerCalculator = (LedgerCalculation) input[3];

            if (this.ledgerBook == null || this.statement == null || this.filter == null || this.filter.Cleared ||
                this.filter.BeginDate == null || this.filter.EndDate == null)
            {
                Enabled = false;
                return;
            }

            if (this.filter.BeginDate.Value.DurationInMonths(this.filter.EndDate.Value) != 1)
            {
                ToolTip = DesignedForOneMonthOnly;
                Enabled = false;
                return;
            }

            Enabled = true;
            var openingBalance = CalculateOpeningBalance();
            var remainingBalance = this.ledgerCalculator.CalculateCurrentMonthSurplusBalance(this.ledgerBook,
                this.filter, this.statement);

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

            ToolTip = string.Format(CultureInfo.CurrentCulture, "Remaining Surplus for period is {0:C} of {1:C}",
                remainingBalance, openingBalance);
        }

        private decimal CalculateOpeningBalance()
        {
            var line = this.ledgerCalculator.LocateApplicableLedgerLine(this.ledgerBook, this.filter);
            return line?.CalculatedSurplus ?? 0;
        }
    }
}