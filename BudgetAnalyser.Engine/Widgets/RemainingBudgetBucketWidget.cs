using System;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Use this widget class for hard coded budget bucket widgets.
    /// </summary>
    public abstract class RemainingBudgetBucketWidget : ProgressBarWidget
    {
        private readonly string standardStyle;
        private IBudgetBucketRepository bucketRepository;
        private StatementModel statement;

        protected RemainingBudgetBucketWidget()
        {
            Category = WidgetGroup.MonthlyTrackingSectionName;
            Dependencies = new[] { typeof(IBudgetCurrencyContext), typeof(StatementModel), typeof(GlobalFilterCriteria), typeof(IBudgetBucketRepository), typeof(LedgerBook), typeof(LedgerCalculation) };
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
            this.standardStyle = "WidgetStandardStyle3";
        }

        protected string BucketCode { get; set; }
        protected IBudgetCurrencyContext Budget { get; private set; }
        protected string DependencyMissingToolTip { get; set; }
        protected GlobalFilterCriteria Filter { get; private set; }
        protected string RemainingBudgetToolTip { get; set; }

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

            Budget = (IBudgetCurrencyContext)input[0];
            this.statement = (StatementModel)input[1];
            Filter = (GlobalFilterCriteria)input[2];
            this.bucketRepository = (IBudgetBucketRepository)input[3];
            var ledgerBook = (LedgerBook)input[4];
            var ledgerCalculation = (LedgerCalculation)input[5];

            if (!this.bucketRepository.IsValidCode(BucketCode))
            {
                Enabled = false;
                return;
            }

            SetAdditionalDependencies(input);

            if (this.statement == null || Budget == null || Filter == null || Filter.Cleared || Filter.BeginDate == null || Filter.EndDate == null || ledgerCalculation == null || ledgerBook == null)
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            decimal totalBudget = MonthlyBudgetAmount()
                                  * Filter.BeginDate.Value.DurationInMonths(Filter.EndDate.Value);
            Maximum = Convert.ToDouble(totalBudget);

            decimal totalSpend = ledgerCalculation.CalculateCurrentMonthBucketSpend(ledgerBook, Filter, this.statement, BucketCode);

            decimal remainingBudget = totalBudget + totalSpend;
            if (remainingBudget < 0)
            {
                remainingBudget = 0;
            }

            Value = Convert.ToDouble(remainingBudget);
            ToolTip = string.Format(CultureInfo.CurrentCulture, RemainingBudgetToolTip, remainingBudget);

            if (remainingBudget < 0.2M * totalBudget)
            {
                ColourStyleName = WidgetWarningStyle;
            }
            else
            {
                ColourStyleName = this.standardStyle;
            }
        }

        protected virtual decimal MonthlyBudgetAmount()
        {
            return Budget.Model.Expenses.Single(b => b.Bucket.Code == BucketCode).Amount;
        }

        protected virtual void SetAdditionalDependencies(object[] input)
        {
        }
    }
}