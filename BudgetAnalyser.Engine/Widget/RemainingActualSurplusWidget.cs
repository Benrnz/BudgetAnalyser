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
        private LedgerBook ledgerBook;
        private string standardStyle;

        public RemainingActualSurplusWidget()
        {
            Category = "Monthly Budget";
            DetailedText = "Surplus A";
            Name = "Surplus A";
            Dependencies = new[] { typeof(StatementModel), typeof(GlobalFilterCriteria), typeof(LedgerBook) };
            this.standardStyle = "WidgetStandardStyle3";
        }

        public override void Update(params object[] input)
        {
            if (!ValidateUpdateInput(input))
            {
                Visibility = false;
                return;
            }

            this.statement = (StatementModel)input[0];
            this.filter = (GlobalFilterCriteria)input[1];
            this.ledgerBook = (LedgerBook)input[2];

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
                ColourStyleName = this.standardStyle;
            }

            ToolTip = string.Format("Remaining Surplus for period is {0:C} of {1:C}", remainingBalance, openingBalance);
        }

        private static decimal CalculateOpeningBalance(GlobalFilterCriteria filter, LedgerBook ledgerBook)
        {
            var line = LedgerCalculation.LocateApplicableLedgerLine(ledgerBook, filter);
            return line == null ? 0 : line.CalculatedSurplus;
        }

        private static decimal CalculateSurplusSpend(StatementModel statementModel)
        {
            return statementModel.Transactions.Where(t => t.BudgetBucket is SurplusBucket).Sum(t => t.Amount);
        }
    }
}
