using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Use this widget base class for widgets that monitor budget bucket spend.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.ProgressBarWidget" />
    public abstract class RemainingBudgetBucketWidget : ProgressBarWidget
    {
        private readonly string standardStyle;
        private IBudgetBucketRepository bucketRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RemainingBudgetBucketWidget" /> class.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the bucket code.
        /// </summary>
        public string BucketCode { get; set; }

        /// <summary>
        ///     Gets the budget model.
        /// </summary>
        protected IBudgetCurrencyContext Budget { get; private set; }

        /// <summary>
        ///     Gets or sets the dependency missing tool tip.
        /// </summary>
        protected string DependencyMissingToolTip { get; set; }

        /// <summary>
        ///     Gets the global filter.
        /// </summary>
        protected GlobalFilterCriteria Filter { get; private set; }

        /// <summary>
        ///     Gets the ledger book model.
        /// </summary>
        protected LedgerBook LedgerBook { get; private set; }

        /// <summary>
        ///     Gets the ledger calculator.
        /// </summary>
        protected LedgerCalculation LedgerCalculation { get; private set; }

        /// <summary>
        ///     Gets or sets the remaining budget tool tip.
        /// </summary>
        protected string RemainingBudgetToolTip { get; set; }

        /// <summary>
        ///     Gets the statement model.
        /// </summary>
        protected StatementModel Statement { get; private set; }

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
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

            Budget = (IBudgetCurrencyContext) input[0];
            Statement = (StatementModel) input[1];
            Filter = (GlobalFilterCriteria) input[2];
            this.bucketRepository = (IBudgetBucketRepository) input[3];
            LedgerBook = (LedgerBook) input[4];
            LedgerCalculation = (LedgerCalculation) input[5];

            SetAdditionalDependencies(input);

            if (Statement == null || Budget == null || Filter == null || Filter.Cleared || Filter.BeginDate == null ||
                Filter.EndDate == null || LedgerCalculation == null || LedgerBook == null || this.bucketRepository == null)
            {
                Enabled = false;
                return;
            }

            if (!this.bucketRepository.IsValidCode(BucketCode))
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
            var totalBudget = MonthlyBudgetAmount();
            Maximum = Convert.ToDouble(totalBudget);

            var totalSpend = LedgerCalculation.CalculateCurrentMonthBucketSpend(LedgerBook, Filter, Statement,
                BucketCode);

            var remainingBalance = totalBudget + totalSpend;
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

        /// <summary>
        ///     Calculates the monthly budget amount.
        /// </summary>
        protected virtual decimal MonthlyBudgetAmount()
        {
            Debug.Assert(Filter.BeginDate != null);
            Debug.Assert(Filter.EndDate != null);

            var monthlyBudget = Budget.Model.Expenses.Single(b => b.Bucket.Code == BucketCode).Amount;
            var totalBudgetedAmount = monthlyBudget;
            var ledgerLine = LedgerCalculation.LocateApplicableLedgerLine(LedgerBook, Filter);

            if (LedgerBook == null || ledgerLine == null ||
                ledgerLine.Entries.All(e => e.LedgerBucket.BudgetBucket.Code != BucketCode))
            {
                return totalBudgetedAmount;
            }

            return ledgerLine.Entries.First(e => e.LedgerBucket.BudgetBucket.Code == BucketCode).Balance;
        }

        /// <summary>
        ///     Provides an optional means to include additional dependencies.
        /// </summary>
        protected virtual void SetAdditionalDependencies(object[] input)
        {
        }
    }
}