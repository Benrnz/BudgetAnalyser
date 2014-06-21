using System;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets
{
    public class SavingsGoalWidget : ProgressBarWidget
    {
        private readonly string standardStyle;

        public SavingsGoalWidget()
        {
            Category = "Monthly Budget";
            Dependencies = new[] { typeof(BudgetCurrencyContext), typeof(StatementModel), typeof(GlobalFilterCriteria) };
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
            this.standardStyle = "WidgetStandardStyle3";
            DetailedText = "Savings Commitment";
            Name = "Save Goal";
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

            var budget = (BudgetCurrencyContext)input[0];
            var statement = (StatementModel)input[1];
            var filter = (GlobalFilterCriteria)input[2];

            if (statement == null || budget == null || filter == null || filter.Cleared || filter.BeginDate == null || filter.EndDate == null)
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            var totalMonths = filter.BeginDate.Value.DurationInMonths(filter.EndDate.Value);
            Maximum = Convert.ToDouble(budget.Model.Expenses.Where(s => s.Bucket is SavingsCommitmentBucket).Sum(s => s.Amount)) * totalMonths;

            // Debit transactions are negative so normally the total spend will be a negative number.
            decimal savingsToDate = -statement.Transactions.Where(t => t.BudgetBucket is SavingsCommitmentBucket && t.Amount < 0).Sum(t => t.Amount);
            if (savingsToDate < 0)
            {
                savingsToDate = 0;
            }

            Value = Convert.ToDouble(savingsToDate);
            ToolTip = string.Format(CultureInfo.CurrentCulture, "You have saved {0:C} of your monthly goal {1:C}", savingsToDate, Maximum);

            if ((double)savingsToDate < 0.8 * Maximum)
            {
                ColourStyleName = WidgetWarningStyle;
            }
            else
            {
                ColourStyleName = this.standardStyle;
            }
        }
    }
}