using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

        protected RemainingBudgetBucketWidget()
        {
            Category = WidgetGroup.MonthlyTrackingSectionName;
            Dependencies = new[]
            {
                typeof(IBudgetCurrencyContext),
                typeof(StatementModel),
                typeof(GlobalFilterCriteria),
                typeof(IBudgetBucketRepository),
                typeof(LedgerBook),
                typeof(LedgerCalculation)
            };
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(6);
            RemainingBudgetToolTip = "Remaining Balance for period is {0:C}";
            this.standardStyle = "WidgetStandardStyle3";
            BucketCode = "<NOT SET>";
        }

        public string BucketCode { get; set; }
        protected IBudgetCurrencyContext Budget { get; private set; }
        protected string DependencyMissingToolTip { get; set; }
        protected GlobalFilterCriteria Filter { get; private set; }
        protected LedgerBook LedgerBook { get; private set; }
        protected LedgerCalculation LedgerCalculation { get; private set; }
        protected string RemainingBudgetToolTip { get; set; }
        protected StatementModel Statement { get; private set; }

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
            Statement = (StatementModel)input[1];
            Filter = (GlobalFilterCriteria)input[2];
            this.bucketRepository = (IBudgetBucketRepository)input[3];
            LedgerBook = (LedgerBook)input[4];
            LedgerCalculation = (LedgerCalculation)input[5];

            if (!this.bucketRepository.IsValidCode(BucketCode))
            {
                Enabled = false;
                return;
            }

            SetAdditionalDependencies(input);

            if (Statement == null || Budget == null || Filter == null || Filter.Cleared || Filter.BeginDate == null || Filter.EndDate == null || LedgerCalculation == null || LedgerBook == null)
            {
                Enabled = false;
                return;
            }

            if (Filter.BeginDate.Value.DurationInMonths(Filter.EndDate.Value) != 1)
            {
                Enabled = false;
                ToolTip = DesignedForOneMonthOnly;
                return;
            }

            Enabled = true;
            decimal totalBudget = MonthlyBudgetAmount()
                                  * Filter.BeginDate.Value.DurationInMonths(Filter.EndDate.Value);
            Maximum = Convert.ToDouble(totalBudget);

            decimal totalSpend = LedgerCalculation.CalculateCurrentMonthBucketSpend(LedgerBook, Filter, Statement, BucketCode);

            decimal remainingBalance = totalBudget + totalSpend;
            if (remainingBalance < 0)
            {
                remainingBalance = 0;
            }

            Value = Convert.ToDouble(remainingBalance);
            ToolTip = string.Format(CultureInfo.CurrentCulture, RemainingBudgetToolTip, remainingBalance);

            if (remainingBalance < 0.2M * totalBudget)
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
            Debug.Assert(Filter.BeginDate != null);
            Debug.Assert(Filter.EndDate != null);

            decimal monthlyBudget = Budget.Model.Expenses.Single(b => b.Bucket.Code == BucketCode).Amount;
            decimal totalBudgetedAmount = monthlyBudget;
            LedgerEntryLine ledgerLine = LedgerCalculation.LocateApplicableLedgerLine(LedgerBook, Filter);

            if (LedgerBook == null || ledgerLine == null || ledgerLine.Entries.All(e => e.LedgerBucket.BudgetBucket.Code != BucketCode))
            {
                return totalBudgetedAmount;
            }

            return ledgerLine.Entries.First(e => e.LedgerBucket.BudgetBucket.Code == BucketCode).Balance;
        }

        protected virtual void SetAdditionalDependencies(object[] input)
        {
        }
    }
}