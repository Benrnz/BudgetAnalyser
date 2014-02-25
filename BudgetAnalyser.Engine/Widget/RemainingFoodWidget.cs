using System;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widget
{
    public class RemainingFoodWidget : Widget
    {
        private BudgetCurrencyContext budget;
        private GlobalFilterCriteria filter;
        private int filterHash;
        private StatementModel statement;

        public RemainingFoodWidget()
        {
            Category = "Monthly Budget";
            Dependencies = new[] { typeof(BudgetCurrencyContext), typeof(StatementModel), typeof(GlobalFilterCriteria) };
            DetailedText = "Food";
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
                this.filter = newFilter;
                this.statement = newStatement;
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

            if (this.statement == null || this.budget == null || this.filter == null || this.filter.Cleared || this.filter.BeginDate == null || this.filter.EndDate == null)
            {
                LargeNumber = "?";
                ToolTip = "A Statement, Budget, or a Filter are not present, remaining food budget cannot be calculated.";
                return;
            }

            decimal totalFoodBudget = this.budget.Model.Expenses.Single(b => b.Bucket.Code == "FOOD").Amount * this.filter.BeginDate.Value.DurationInMonths(this.filter.EndDate.Value);

            // Debit transactions are negative so normally the total spend will be a negative number.
            decimal remainingBudget = totalFoodBudget + this.statement.Transactions.Where(t => t.BudgetBucket != null && t.BudgetBucket.Code == "FOOD").Sum(t => t.Amount);
            LargeNumber = remainingBudget.ToString("C");
            ToolTip = string.Format(CultureInfo.CurrentCulture, "Remaining Food budget for period is {0:C}", remainingBudget);
        }
    }
}