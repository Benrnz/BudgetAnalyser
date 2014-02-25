using System;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widget
{
    public class RemainingSurplusWidget : Widget
    {
        private BudgetCurrencyContext budget;
        private GlobalFilterCriteria filter;
        private StatementModel statement;

        public RemainingSurplusWidget()
        {
            Category = "Monthly Budget";
            Dependencies = new[] { typeof(BudgetCurrencyContext), typeof(StatementModel), typeof(GlobalFilterCriteria) };
            DetailedText = "Surplus";
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
        }

        public override void Update(params object[] input)
        {
            if (!ValidateUpdateInput(input))
            {
                return;
            }

            var newBudget = (BudgetCurrencyContext)input[0];
            var newStatement = (StatementModel)input[1];
            var newFilter = (GlobalFilterCriteria)input[2];

            bool updated = false;
            if (newBudget != this.budget)
            {
                this.budget = newBudget;
                updated = true;
            }

            if (newStatement != this.statement)
            {
                this.statement = newStatement;
                updated = true;
            }

            if (newFilter != this.filter)
            {
                this.filter = newFilter;
                updated = true;
            }

            if (!updated)
            {
                return;
            }

            if (this.statement == null || this.budget == null || this.filter == null || this.filter.Cleared || this.filter.BeginDate == null || this.filter.EndDate == null)
            {
                LargeNumber = "?";
                ToolTip = "A Statement, Budget, or a Filter are not present, surplus cannot be calculated.";
                return;
            }

            decimal totalSurplus = this.budget.Model.Surplus*this.filter.BeginDate.Value.DurationInMonths(this.filter.EndDate.Value);

            // Debit transactions are negative so normally the total surplus spend will be a negative number.
            decimal remainingSurplus = totalSurplus + this.statement.Transactions.Where(t => t.BudgetBucket is SurplusBucket).Sum(t => t.Amount);
            LargeNumber = remainingSurplus.ToString("C");
            ToolTip = string.Format(CultureInfo.CurrentCulture, "Remaining Surplus for period is {0:C}", remainingSurplus);
        }
    }
}