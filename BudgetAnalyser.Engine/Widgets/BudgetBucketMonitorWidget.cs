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
    public sealed class BudgetBucketMonitorWidget : ProgressBarWidget, IUserDefinedWidget
    {
        private readonly string disabledToolTip;
        private readonly string remainingBudgetToolTip;
        private readonly string standardStyle;
        private IBudgetBucketRepository bucketRepository;
        private IBudgetCurrencyContext budget;
        private string doNotUseBucketCode;
        private string doNotUseId;
        private GlobalFilterCriteria filter;
        private LedgerBook ledgerBook;
        private LedgerCalculation ledgerCalculator;
        private StatementModel statement;

        public BudgetBucketMonitorWidget()
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
            this.standardStyle = "WidgetStandardStyle3";

            this.disabledToolTip = "Either a Statement, Budget, or a Filter are not present, or the Bucket Code is not valid, remaining budget cannot be calculated.";
            this.remainingBudgetToolTip = "Remaining budget for period is {0:C}";
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

        public  Type WidgetType => GetType();

        public void Initialise(MultiInstanceWidgetState state, ILogger logger)
        {
        }

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

            this.budget = (IBudgetCurrencyContext)input[0];
            this.statement = (StatementModel)input[1];
            this.filter = (GlobalFilterCriteria)input[2];
            this.bucketRepository = (IBudgetBucketRepository)input[3];
            this.ledgerBook = (LedgerBook)input[4];
            this.ledgerCalculator = (LedgerCalculation)input[5];

            if (!this.bucketRepository.IsValidCode(BucketCode))
            {
                ToolTip = this.disabledToolTip;
                Enabled = false;
                return;
            }

            if (this.statement == null || this.budget == null || this.filter == null || this.filter.Cleared || this.filter.BeginDate == null || this.filter.EndDate == null)
            {
                ToolTip = this.disabledToolTip;
                Enabled = false;
                return;
            }

            if (this.filter.BeginDate.Value.DurationInMonths(this.filter.EndDate.Value) != 1)
            {
                Enabled = false;
                ToolTip = DesignedForOneMonthOnly;
                return;
            }

            Enabled = true;
            decimal totalBudget = MonthlyBudgetAmount();
            Maximum = Convert.ToDouble(totalBudget);

            // Debit transactions are negative so normally the total spend will be a negative number.
            decimal remainingBudget = totalBudget + this.statement.Transactions.Where(t => t.BudgetBucket != null && t.BudgetBucket.Code == BucketCode).Sum(t => t.Amount);
            if (remainingBudget < 0)
            {
                remainingBudget = 0;
            }

            Value = Convert.ToDouble(remainingBudget);
            ToolTip = string.Format(CultureInfo.CurrentCulture, this.remainingBudgetToolTip, remainingBudget);

            if (remainingBudget < 0.2M * totalBudget)
            {
                ColourStyleName = WidgetWarningStyle;
            }
            else
            {
                ColourStyleName = this.standardStyle;
            }
        }

        private decimal MonthlyBudgetAmount()
        {
            Debug.Assert(this.filter.BeginDate != null);
            Debug.Assert(this.filter.EndDate != null);

            decimal monthlyBudget = this.budget.Model.Expenses.Single(b => b.Bucket.Code == BucketCode).Amount;
            decimal totalBudgetedAmount = monthlyBudget;
            LedgerEntryLine ledgerLine = this.ledgerCalculator.LocateApplicableLedgerLine(this.ledgerBook, this.filter);

            if (this.ledgerBook == null || ledgerLine == null || ledgerLine.Entries.All(e => e.LedgerBucket.BudgetBucket.Code != BucketCode))
            {
                return totalBudgetedAmount;
            }

            return ledgerLine.Entries.First(e => e.LedgerBucket.BudgetBucket.Code == BucketCode).Balance;
        }
    }
}