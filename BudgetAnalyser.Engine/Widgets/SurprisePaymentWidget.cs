using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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
        private const int FortnightlyPaymentsInOneNormalMonth = 2;

        private const string ToolTipPrefix =
            "Given payments happen on the same day every week, this widget shows which months will require 5 weekly payments instead of 4, or 3 fortnightly payments instead of 2. (Does account for NZ public holidays.)\n";

        private const int WeeklyPaymentsInOneNormalMonth = 4;
        private IBudgetBucketRepository bucketRepository;
        private ILogger diagLogger;
        private WeeklyOrFortnightly doNotUseFrequency;
        private string doNotUseId;
        private GlobalFilterCriteria filter;
        private int multiplier = 1;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SurprisePaymentWidget" /> class.
        /// </summary>
        public SurprisePaymentWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[] { typeof(IBudgetBucketRepository), typeof(GlobalFilterCriteria) };
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
            ToolTip = ToolTipPrefix;
            Size = WidgetSize.Medium;
            WidgetStyle = "ModernTileMediumStyle2";
            ImageResourceName = "BooksImage";
            ImageResourceName2 = "OctWarningImage";
        }

        /// <summary>
        ///     Gets the bucket code.
        /// </summary>
        public string BucketCode => Id;

        /// <summary>
        ///     Gets or sets the frequency of the expected payment.
        /// </summary>
        public WeeklyOrFortnightly Frequency
        {
            get { return this.doNotUseFrequency; }
            set
            {
                this.doNotUseFrequency = value;
                this.multiplier = Frequency == WeeklyOrFortnightly.Weekly ? 1 : 2;
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
            }
        }

        /// <summary>
        ///     Gets or sets the start payment date.
        /// </summary>
        public DateTime StartPaymentDate { get; set; }

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
            var myState = (SurprisePaymentWidgetPersistentState) state;
            StartPaymentDate = myState.PaymentStartDate;
            Frequency = myState.Frequency;
            this.diagLogger = logger;
        }

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)", Justification = "Only a month name is required.")]
        public override void Update(params object[] input)
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

            this.bucketRepository = (IBudgetBucketRepository) input[0];
            this.filter = (GlobalFilterCriteria) input[1];

            if (this.filter == null || this.filter.Cleared || this.filter.BeginDate == null || this.filter.BeginDate == DateTime.MinValue ||
                this.filter.EndDate == null || this.filter.EndDate.Value == DateTime.MinValue || this.bucketRepository == null)
            {
                Enabled = false;
                return;
            }

            if (!this.bucketRepository.IsValidCode(BucketCode))
            {
                Enabled = false;
                LargeNumber = string.Empty;
                ToolTip = string.Empty;
                DetailedText = string.Empty;
                return;
            }

            this.diagLogger.LogInfo(l => l.Format("{0} Calculating Payment Plan for {1}. From {2} to {3}", WidgetType.Name, Id, this.filter.BeginDate, this.filter.EndDate));
            var currentDate = CalculateStartDate(StartPaymentDate, this.filter.BeginDate.Value);
            var content = new StringBuilder();
            // Ignore start date in filter and force it to be one month prior to end date in filter.
            var currentMonthTally = new NextOccurance
            {
                StartDate = this.filter.EndDate.Value.AddDays(1).AddMonths(-1),
                EndDate = this.filter.EndDate.Value
            };
            var alert = false;
            NextOccurance firstOccurance = null;
            do
            {
                if (currentDate.Date <= currentMonthTally.EndDate)
                {
                    currentMonthTally.Tally(currentDate.Date.Day);
                }
                else
                {
                    this.diagLogger.LogInfo(
                        l =>
                            l.Format("    {0} {1}", currentMonthTally.StartDate.ToString("MMMM"),
                                currentMonthTally.ConcatDates()));
                    if (AbnormalNumberOfPayments(currentMonthTally.Dates.Count))
                    {
                        if (firstOccurance == null)
                        {
                            firstOccurance = currentMonthTally;
                        }
                        content.AppendFormat(CultureInfo.CurrentCulture, "{0}, ",
                            currentMonthTally.StartDate.ToString("MMMM"));
                        if (currentMonthTally.EndDate == this.filter.EndDate.Value ||
                            currentMonthTally.EndDate == this.filter.EndDate.Value.AddMonths(1))
                        {
                            // Is current or next month, so signal alert status
                            alert = true;
                            this.diagLogger.LogInfo(l => l.Format("    ***** ALERT *****"));
                        }
                    }
                    currentMonthTally = currentMonthTally.NextMonth(currentDate.Date.Day);
                }

                currentDate = CalculateNextPaymentDate(currentDate);
            } while (currentDate.Date <= this.filter.EndDate.Value.AddYears(1));

            ColourStyleName = alert ? WidgetWarningStyle : WidgetStandardStyle;
            DetailedText = string.Format(CultureInfo.CurrentCulture, "Monitoring {0} {1} bucket. {2}", Frequency,
                BucketCode, content);
            if (firstOccurance == null)
            {
                LargeNumber = string.Empty;
                ToolTip = ToolTipPrefix;
            }
            else
            {
                LargeNumber = firstOccurance.StartDate.ToString("MMMM");
                ToolTip = ToolTipPrefix +
                          string.Format(
                              CultureInfo.InvariantCulture,
                              "{0} to the {1} has payments on {2}",
                              firstOccurance.StartDate.ToString("d-MMM"),
                              firstOccurance.EndDate.ToString("d-MMM"),
                              firstOccurance.ConcatDates());
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

        private PaymentDate CalculateNextPaymentDate(PaymentDate paymentDate)
        {
            var proposedDate = new PaymentDate(paymentDate.ScheduledDate.AddDays(7 * this.multiplier));
            if (this.filter.BeginDate != null)
            {
                List<DateTime> holidays =
                    NewZealandPublicHolidays.CalculateHolidays(this.filter.BeginDate.Value,
                        this.filter.BeginDate.Value.AddYears(1)).ToList();
                while (holidays.Contains(proposedDate.Date))
                {
                    proposedDate.Date = proposedDate.Date.AddDays(1);
                    proposedDate.Date = proposedDate.Date.FindNextWeekday();
                }
            }

            if (proposedDate.Date != proposedDate.ScheduledDate)
            {
                this.diagLogger.LogInfo(
                    l => l.Format("    {0} is a holiday, moved to {1}", proposedDate.ScheduledDate, proposedDate.Date));
            }
            return proposedDate;
        }

        private PaymentDate CalculateStartDate(DateTime startPaymentDate, DateTime filterBeginDate)
        {
            var proposed = new PaymentDate(startPaymentDate);
            while (proposed.Date < filterBeginDate)
            {
                proposed = CalculateNextPaymentDate(proposed);
            }

            this.diagLogger.LogInfo(
                l => l.Format("   Payment Start Date: {0} ({1})", proposed.Date, proposed.ScheduledDate));
            return proposed;
        }

        private class NextOccurance
        {
            public NextOccurance()
            {
                Dates = new List<int>();
            }

            public List<int> Dates { get; }
            public DateTime EndDate { get; set; }
            public DateTime StartDate { get; set; }

            public string ConcatDates()
            {
                var builder = new StringBuilder();
                foreach (var date in Dates)
                {
                    builder.AppendFormat(CultureInfo.CurrentCulture, ", {0}", date);
                }

                if (builder.Length > 0 && builder[0] == ',')
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

        private class PaymentDate
        {
            public PaymentDate(DateTime initial)
            {
                ScheduledDate = initial;
                Date = initial;
            }

            public DateTime Date { get; set; }
            public DateTime ScheduledDate { get; }
        }
    }
}