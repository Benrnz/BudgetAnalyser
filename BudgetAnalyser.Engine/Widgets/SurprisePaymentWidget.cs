using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A widget designed to show any month where there will be 5 weekly payments in one month, or 3 fortnightly payments
    ///     in one month.
    /// </summary>
    public class SurprisePaymentWidget : Widget, IUserDefinedWidget
    {
        private const int WeeklyPaymentsInOneNormalMonth = 4;
        private const int FortnightlyPaymentsInOneNormalMonth = 2;

        private const string ToolTipPrefix =
            "Given payments happen on the same day every week, this widget shows which months will require 5 weekly payments instead of 4, or 3 fortnightly payments instead of 2.\n";

        private IBudgetBucketRepository bucketRepository;
        private WeeklyOrFortnightly doNotUseFrequency;
        private string doNotUseId;
        private GlobalFilterCriteria filter;
        private int multiplier = 1;

        public SurprisePaymentWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[] { typeof(IBudgetBucketRepository), typeof(GlobalFilterCriteria) };
            DetailedText = "Additional weekly payment required";
            ImageResourceName = null;
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
            ToolTip = ToolTipPrefix;
            Size = WidgetSize.Medium;
            WidgetStyle = "ModernTileMediumStyle2";
        }

        public string BucketCode
        {
            get { return Id; }
        }

        public WeeklyOrFortnightly Frequency
        {
            get { return this.doNotUseFrequency; }
            set
            {
                this.doNotUseFrequency = value;
                this.multiplier = Frequency == WeeklyOrFortnightly.Weekly ? 1 : 2;
            }
        }

        public string Id
        {
            get { return this.doNotUseId; }
            set
            {
                this.doNotUseId = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartPaymentDate { get; set; }

        public Type WidgetType
        {
            get { return GetType(); }
        }

        public void Initialise(MultiInstanceWidgetState state)
        {
            var myState = (SurprisePaymentWidgetPersistentState)state;
            StartPaymentDate = myState.PaymentStartDate;
            Frequency = myState.Frequency;
        }

        public override void Update(params object[] input)
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

            this.bucketRepository = (IBudgetBucketRepository)input[0];
            this.filter = (GlobalFilterCriteria)input[1];

            if (!this.bucketRepository.IsValidCode(BucketCode))
            {
                Enabled = false;
                return;
            }

            if (this.filter.Cleared || this.filter.BeginDate == null || this.filter.BeginDate == DateTime.MinValue || this.filter.EndDate == null || this.filter.EndDate.Value == DateTime.MinValue)
            {
                Enabled = false;
                return;
            }

            var currentDate = CalculateStartDate(StartPaymentDate, this.filter.BeginDate.Value);
            var content = new StringBuilder();
            // Ignore start date in filter and force it to be one month prior to end date in filter.
            var currentMonthTally = new NextOccurance { StartDate = this.filter.EndDate.Value.AddDays(1).AddMonths(-1), EndDate = this.filter.EndDate.Value };
            var alert = false;
            NextOccurance firstOccurance = null;
            do
            {
                if (currentDate <= currentMonthTally.EndDate)
                {
                    currentMonthTally.Tally(currentDate.Day);
                }
                else
                {
                    if (AbnormalNumberOfPayments(currentMonthTally.Dates.Count))
                    {
                        if (firstOccurance == null)
                        {
                            firstOccurance = currentMonthTally;
                        }
                        content.AppendFormat("{0},", currentMonthTally.StartDate.ToString("MMMM"));
                        if (currentMonthTally.EndDate == this.filter.EndDate.Value || currentMonthTally.EndDate == this.filter.EndDate.Value.AddMonths(1))
                        {
                            // Is current or next month, so signal alert status
                            alert = true;
                        }
                    }
                    currentMonthTally = currentMonthTally.NextMonth(currentDate.Day);
                }

                currentDate = currentDate.AddDays(7 * this.multiplier);
            } while (currentDate < this.filter.BeginDate.Value.AddYears(1));

            ColourStyleName = alert ? WidgetWarningStyle : WidgetStandardStyle;
            DetailedText = string.Format(CultureInfo.CurrentCulture, "Monitoring {0} {1} bucket. {2}", Frequency, BucketCode, content);
            if (firstOccurance == null)
            {
                LargeNumber = string.Empty;
                ToolTip = ToolTipPrefix;
            }
            else
            {
                LargeNumber = firstOccurance.StartDate.ToString("MMMM");
                ToolTip = ToolTipPrefix +
                          string.Format("{0} to the {1} has payments on {2}", firstOccurance.StartDate.ToString("d-MMM"), firstOccurance.EndDate.ToString("d-MMM"), firstOccurance.ConcatDates());
            }
        }

        private bool AbnormalNumberOfPayments(int paymentsInMonthCount)
        {
            switch (Frequency)
            {
                case WeeklyOrFortnightly.Weekly:
                    return paymentsInMonthCount > WeeklyPaymentsInOneNormalMonth;
                case WeeklyOrFortnightly.Fortnightly:
                    return paymentsInMonthCount > FortnightlyPaymentsInOneNormalMonth;
                default:
                    throw new NotSupportedException("Unexpected frequency enumeration value found: " + Frequency);
            }
        }

        private DateTime CalculateStartDate(DateTime startPaymentDate, DateTime filterBeginDate)
        {
            while (startPaymentDate < filterBeginDate)
            {
                startPaymentDate = startPaymentDate.AddDays(7 * this.multiplier);
            }
            return startPaymentDate;
        }

        private class NextOccurance
        {
            public NextOccurance()
            {
                Dates = new List<int>();
            }

            public List<int> Dates { get; private set; }
            public DateTime EndDate { get; set; }
            public DateTime StartDate { get; set; }

            public string ConcatDates()
            {
                var builder = new StringBuilder();
                foreach (var date in Dates)
                {
                    builder.AppendFormat(", {0}", date);
                }

                if (builder[0] == ',')
                {
                    builder.Remove(0, 1);
                    builder.Remove(0, 1);
                }

                return builder.ToString();
            }

            public NextOccurance NextMonth(int day)
            {
                var nextMonth = new NextOccurance { StartDate = StartDate.AddMonths(1), EndDate = EndDate.AddMonths(1) };
                nextMonth.Tally(day);
                return nextMonth;
            }

            public void Tally(int day)
            {
                Dates.Add(day);
            }
        }
    }
}