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
        private IBudgetBucketRepository bucketRepository;
        private string doNotUseBucketCode;
        private string doNotUseId;
        private StatementModel statement;
        private readonly string disabledToolTip;
        private readonly string remainingBudgetToolTip;
        private readonly string standardStyle;

        public FixedBudgetMonitorWidget()
        {
            Category = "3 Monthly Tracking";
            Dependencies = new[] { typeof(StatementModel), typeof(IBudgetBucketRepository) };
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(6);
            this.standardStyle = "WidgetStandardStyle3";

            this.disabledToolTip = "No Statement file is loaded, or bucket doesn't exist.";
            this.remainingBudgetToolTip = "{0} Remaining budget for this project: {1:C}";
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

        public Type WidgetType
        {
            get { return GetType(); }
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

            this.statement = (StatementModel)input[0];
            this.bucketRepository = (IBudgetBucketRepository)input[1];

            if (!this.bucketRepository.IsValidCode(BucketCode))
            {
                ToolTip = this.disabledToolTip;
                Enabled = false;
                return;
            }

            if (this.statement == null)
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
            var remainingBudget = totalBudget + this.statement.AllTransactions.Where(t => t.BudgetBucket != null && t.BudgetBucket.Code == BucketCode).Sum(t => t.Amount);

            Value = Convert.ToDouble(remainingBudget);
            ToolTip = string.Format(CultureInfo.CurrentCulture, this.remainingBudgetToolTip, bucket.Description, remainingBudget);
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