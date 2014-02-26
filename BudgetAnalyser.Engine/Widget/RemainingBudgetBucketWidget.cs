using System;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widget
{
    public abstract class RemainingBudgetBucketWidget : Widget
    {
        private IBudgetBucketRepository bucketRepository;
        private GlobalFilterCriteria filter;
        private int filterHash;
        private StatementModel statement;

        protected RemainingBudgetBucketWidget()
        {
            Category = "Monthly Budget";
            Dependencies = new[] { typeof(BudgetCurrencyContext), typeof(StatementModel), typeof(GlobalFilterCriteria), typeof(IBudgetBucketRepository) };
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
        }

        protected string BucketCode { get; set; }
        protected BudgetCurrencyContext Budget { get; private set; }
        protected string DependencyMissingToolTip { get; set; }
        protected string RemainingBudgetToolTip { get; set; }

        public override void Update(params object[] input)
        {
            if (!ValidateUpdateInput(input))
            {
                Visibility = false;
                return;
            }

            var newBudget = (BudgetCurrencyContext)input[0];
            var newStatement = (StatementModel)input[1];
            var newFilter = (GlobalFilterCriteria)input[2];
            this.bucketRepository = (IBudgetBucketRepository)input[3];

            if (!this.bucketRepository.IsValidCode(BucketCode))
            {
                Visibility = false;
                return;
            }

            bool updated = false;
            if (newBudget != Budget)
            {
                Budget = newBudget;
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

            if (this.statement == null || Budget == null || this.filter == null || this.filter.Cleared || this.filter.BeginDate == null || this.filter.EndDate == null)
            {
                Visibility = false;
                LargeNumber = "?";
                ToolTip = DependencyMissingToolTip;
                return;
            }

            Visibility = true;
            decimal totalFoodBudget = MonthlyBudgetAmount()
                                      *this.filter.BeginDate.Value.DurationInMonths(this.filter.EndDate.Value);

            // Debit transactions are negative so normally the total spend will be a negative number.
            decimal remainingBudget = totalFoodBudget + this.statement.Transactions.Where(t => t.BudgetBucket != null && t.BudgetBucket.Code == BucketCode).Sum(t => t.Amount);
            LargeNumber = remainingBudget.ToString("C");
            ToolTip = string.Format(CultureInfo.CurrentCulture, RemainingBudgetToolTip, remainingBudget);
        }

        protected virtual decimal MonthlyBudgetAmount()
        {
            return Budget.Model.Expenses.Single(b => b.Bucket.Code == BucketCode).Amount;
        }
    }
}