using System;
using System.Globalization;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets
{
    public class RemainingActualSurplusWidget : ProgressBarWidget
    {
        private readonly string standardStyle;
        private GlobalFilterCriteria filter;
        private LedgerBook ledgerBook;
        private StatementModel statement;

        public RemainingActualSurplusWidget()
        {
            Category = "Monthly Budget";
            DetailedText = "Bank Surplus";
            Name = "Surplus A";
            Dependencies = new[] { typeof(StatementModel), typeof(GlobalFilterCriteria), typeof(LedgerBook) };
            this.standardStyle = "WidgetStandardStyle3";
        }

        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (!ValidateUpdateInput(input))
            {
                Enabled = false;
                return;
            }

            this.statement = (StatementModel)input[0];
            this.filter = (GlobalFilterCriteria)input[1];
            this.ledgerBook = (LedgerBook)input[2];

            if (this.ledgerBook == null || this.statement == null || this.filter == null || this.filter.Cleared || this.filter.BeginDate == null || this.filter.EndDate == null)
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
            decimal openingBalance = CalculateOpeningBalance(this.filter, this.ledgerBook);
            decimal remainingBalance = LedgerCalculation.CalculateCurrentMonthSurplusBalance(this.ledgerBook, this.filter, this.statement);

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

            ToolTip = string.Format(CultureInfo.CurrentCulture, "Remaining Surplus for period is {0:C} of {1:C}", remainingBalance, openingBalance);
        }

        private static decimal CalculateOpeningBalance(GlobalFilterCriteria filter, LedgerBook ledgerBook)
        {
            LedgerEntryLine line = LedgerCalculation.LocateApplicableLedgerLine(ledgerBook, filter);
            return line == null ? 0 : line.CalculatedSurplus;
        }
    }
}