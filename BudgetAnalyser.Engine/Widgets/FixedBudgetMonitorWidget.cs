using System;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets
{
    public sealed class FixedBudgetMonitorWidget : ProgressBarWidget, IUserDefinedWidget
    {
        private readonly string disabledToolTip;
        private readonly string remainingBudgetToolTip;
        private readonly string standardStyle;
        private IBudgetBucketRepository bucketRepository;
        private string doNotUseBucketCode;
        private string doNotUseId;

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

        public StatementModel Statement { get; private set; }

        public Type WidgetType
        {
            get { return GetType(); }
        }

        public void Initialise(MultiInstanceWidgetState state, ILogger logger)
        {
        }

        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (!ValidateUpdateInput(input))
            {
                ToolTip = this.disabledToolTip;
                Enabled = false;
                return;
            }

            Statement = (StatementModel)input[0];
            this.bucketRepository = (IBudgetBucketRepository)input[1];

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
            var bucket = (FixedBudgetProjectBucket)this.bucketRepository.GetByCode(BucketCode);
            var totalBudget = bucket.FixedBudgetAmount;
            Maximum = Convert.ToDouble(totalBudget);

            // Debit transactions are negative so normally the total spend will be a negative number.
            var totalSpend = Statement.AllTransactions.Where(t => t.BudgetBucket != null && t.BudgetBucket.Code == BucketCode).Sum(t => t.Amount);
            var remainingBudget = totalBudget + totalSpend;

            Value = Convert.ToDouble(remainingBudget);
            ToolTip = string.Format(CultureInfo.CurrentCulture, this.remainingBudgetToolTip, bucket.Description, remainingBudget, totalSpend);
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