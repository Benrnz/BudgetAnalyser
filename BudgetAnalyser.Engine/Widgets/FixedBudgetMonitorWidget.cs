using System;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A fixed budget project monitor widget.  Used to monitor spend for a <see cref="FixedBudgetProjectBucket" />.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.ProgressBarWidget" />
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.IUserDefinedWidget" />
    public sealed class FixedBudgetMonitorWidget : ProgressBarWidget, IUserDefinedWidget
    {
        private readonly string disabledToolTip;
        private readonly string remainingBudgetToolTip;
        private readonly string standardStyle;
        private IBudgetBucketRepository bucketRepository;
        private string doNotUseBucketCode;
        private string doNotUseId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FixedBudgetMonitorWidget" /> class.
        /// </summary>
        public FixedBudgetMonitorWidget()
        {
            Category = WidgetGroup.ProjectsSectionName;
            Dependencies = new[] { typeof(StatementModel), typeof(IBudgetBucketRepository) };
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(6);
            this.standardStyle = "WidgetStandardStyle1";

            this.disabledToolTip = "No Statement file is loaded, or bucket doesn't exist.";
            this.remainingBudgetToolTip = "{0} Remaining budget for this project: {1:C}. Total Spend {2:C}";
            Enabled = false;
            BucketCode = "<NOT SET>";
        }

        /// <summary>
        ///     Gets or sets the bucket code.
        /// </summary>
        public string BucketCode
        {
            get { return this.doNotUseBucketCode; }
            set
            {
                this.doNotUseBucketCode = value;
                OnPropertyChanged();
                DetailedText = BucketCode;
            }
        }

        /// <summary>
        ///     Gets or sets a unique identifier for the widget. This is required for persistence purposes.
        /// </summary>
        public string Id
        {
            get { return this.doNotUseId; }
            set
            {
                this.doNotUseId = value;
                OnPropertyChanged();
                BucketCode = Id;
            }
        }

        /// <summary>
        ///     Gets the statement model.
        /// </summary>
        public StatementModel Statement { get; private set; }

        /// <summary>
        ///     Gets the type of the widget. Optionally allows the implementation to override the widget type description used in
        ///     the user interface.
        /// </summary>
        public Type WidgetType => GetType();

        /// <summary>
        ///     Initialises the widget and optionally offers it some state and a logger.
        /// </summary>
        public void Initialise(MultiInstanceWidgetState state, ILogger logger)
        {
        }

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
                ToolTip = this.disabledToolTip;
                Enabled = false;
                return;
            }

            Statement = (StatementModel) input[0];
            this.bucketRepository = (IBudgetBucketRepository) input[1];

            if (!this.bucketRepository.IsValidCode(BucketCode))
            {
                ToolTip = this.disabledToolTip;
                Enabled = false;
                return;
            }

            if (Statement == null)
            {
                ToolTip = this.disabledToolTip;
                Enabled = false;
                return;
            }

            Enabled = true;
            var bucket = (FixedBudgetProjectBucket) this.bucketRepository.GetByCode(BucketCode);
            var totalBudget = bucket.FixedBudgetAmount;
            Maximum = Convert.ToDouble(totalBudget);

            // Debit transactions are negative so normally the total spend will be a negative number.
            var totalSpend =
                Statement.AllTransactions.Where(t => t.BudgetBucket != null && t.BudgetBucket.Code == BucketCode)
                    .Sum(t => t.Amount);
            var remainingBudget = totalBudget + totalSpend;

            Value = Convert.ToDouble(remainingBudget);
            ToolTip = string.Format(CultureInfo.CurrentCulture, this.remainingBudgetToolTip, bucket.Description,
                remainingBudget, totalSpend);
            DetailedText = string.Format(CultureInfo.CurrentCulture, "{0} Project", bucket.SubCode);

            if (remainingBudget < 0.1M * totalBudget)
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