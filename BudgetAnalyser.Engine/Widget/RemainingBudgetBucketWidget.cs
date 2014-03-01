using System;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widget
{
    public abstract class RemainingBudgetBucketWidget : ProgressBarWidget
    {
        private IBudgetBucketRepository bucketRepository;
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
        protected GlobalFilterCriteria Filter { get; private set; }
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
                Filter = newFilter;
                this.statement = newStatement;
                updated = true;
            }

            if (newFilter.GetHashCode() != this.filterHash)
            {
                this.filterHash = newFilter.GetHashCode();
                updated = true;
            }

            if (SetAdditionalDependencies(input))
            {
                updated = true;
            }

            if (!updated)
            {
                return;
            }

            if (this.statement == null || Budget == null || Filter == null || Filter.Cleared || Filter.BeginDate == null || Filter.EndDate == null)
            {
                Visibility = false;
                LargeNumber = "?";
                ToolTip = DependencyMissingToolTip;
                return;
            }

            Visibility = true;
            decimal totalBudget = MonthlyBudgetAmount()
                                      *Filter.BeginDate.Value.DurationInMonths(Filter.EndDate.Value);
            Maximum = Convert.ToDouble(totalBudget);

            // Debit transactions are negative so normally the total spend will be a negative number.
            decimal remainingBudget = totalBudget + this.statement.Transactions.Where(t => t.BudgetBucket != null && t.BudgetBucket.Code == BucketCode).Sum(t => t.Amount);
            
            Value = Convert.ToDouble(remainingBudget);
            ToolTip = string.Format(CultureInfo.CurrentCulture, RemainingBudgetToolTip, remainingBudget);

            if (remainingBudget < 0.8M*totalBudget)
            {
                ColourStyleName = WidgetWarningStyle;
            }
            else
            {
                ColourStyleName = WidgetStandardStyle;
            }
        }

        protected virtual decimal MonthlyBudgetAmount()
        {
            return Budget.Model.Expenses.Single(b => b.Bucket.Code == BucketCode).Amount;
        }

        protected virtual bool SetAdditionalDependencies(object[] input)
        {
            return false;
        }
    }
}