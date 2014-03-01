using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widget
{
    public class RemainingActualSurplusWidget : ProgressBarWidget
    {
        private StatementModel statement;
        private GlobalFilterCriteria filter;
        private int filterHash;
        private LedgerBook ledgerBook;

        public RemainingActualSurplusWidget()
        {
            Category = "Monthly Budget";
            DetailedText = "Surplus A";
            Dependencies = new[] { typeof(StatementModel), typeof(GlobalFilterCriteria), typeof(LedgerBook) };
        }

        public override void Update(params object[] input)
        {
            if (!ValidateUpdateInput(input))
            {
                Visibility = false;
                return;
            }

            var newStatement = (StatementModel)input[0];
            var newFilter = (GlobalFilterCriteria)input[1];
            var newLedgerBook = (LedgerBook)input[2];

            bool updated = false;
            if (newStatement != this.statement)
            {
                this.filter = newFilter;
                this.statement = newStatement;
                updated = true;
            }

            if (newLedgerBook != this.ledgerBook)
            {
                this.ledgerBook = newLedgerBook;
                updated = true;
            }

            if (newFilter.GetHashCode() != this.filterHash)
            {
                this.filterHash = newFilter.GetHashCode();
                updated = true;
            }

            if (!updated)
            {
                return;
            }

            if (this.ledgerBook == null || this.statement == null || this.filter == null || this.filter.Cleared || this.filter.BeginDate == null || this.filter.EndDate == null)
            {
                Visibility = false;
                return;
            }

            Visibility = true;
            var openingBalance = CalculateOpeningBalance(this.filter, this.ledgerBook);
            var remainingBalance = openingBalance + CalculateSurplusSpend(this.statement);

            Maximum = Convert.ToDouble(openingBalance);
            Value = Convert.ToDouble(remainingBalance);
            Minimum = 0;
            if (remainingBalance < 0.2M*openingBalance)
            {
                ColourStyleName = WidgetWarningStyle;
            }
            else
            {
                ColourStyleName = WidgetStandardStyle;
            }

            ToolTip = string.Format("Remaining Surplus for period is {0:C}", remainingBalance);
        }

        private static decimal CalculateOpeningBalance(GlobalFilterCriteria filter, LedgerBook ledgerBook)
        {
            var line = LedgerCalculation.LocateApplicableLedgerLine(ledgerBook, filter);
            return line.CalculatedSurplus;
        }

        private static decimal CalculateSurplusSpend(StatementModel statementModel)
        {
            return statementModel.Transactions.Where(t => t.BudgetBucket is SurplusBucket).Sum(t => t.Amount);
        }
    }
}
