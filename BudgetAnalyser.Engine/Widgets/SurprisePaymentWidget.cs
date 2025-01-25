using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A widget designed to show any month, where there will be 5 weekly payments in one month, or 3 fortnightly payments in one month.
///     When using a fortnightly budget period this widget is irrelevant.
/// </summary>
public class SurprisePaymentWidget : Widget, IUserDefinedWidget
{
    private const int FortnightlyPaymentsInOneNormalMonth = 2;

    private const string ToolTipPrefix = "Given payments happen on the same day every week, this widget shows which months will require 5 weekly payments instead of 4, or 3 fortnightly payments instead of 2. (Does account for NZ public holidays.)\n";

    private const int WeeklyPaymentsInOneNormalMonth = 4;
    private IBudgetBucketRepository? bucketRepository;
    private ILogger? diagLogger;
    private WeeklyOrFortnightly doNotUseFrequency;
    private string doNotUseId = NotSet;
    private GlobalFilterCriteria? filter;
    private int multiplier = 1;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SurprisePaymentWidget" /> class.
    /// </summary>
    public SurprisePaymentWidget()
    {
        Category = WidgetGroup.OverviewSectionName;
        Dependencies = [typeof(IBudgetBucketRepository), typeof(GlobalFilterCriteria), typeof(BudgetCollection)];
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
        get => this.doNotUseFrequency;
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
        get => this.doNotUseId;
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
        var myState = (SurprisePaymentWidgetPersistentState)state;
        StartPaymentDate = myState.PaymentStartDate;
        Frequency = myState.Frequency;
        this.diagLogger = logger;
    }

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)", Justification = "Only a month name is required.")]
    public override void Update(params object[] input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (!ValidateUpdateInput(input))
        {
            Enabled = false;
            return;
        }

        this.bucketRepository = (IBudgetBucketRepository)input[0];
        this.filter = input[1] as GlobalFilterCriteria;
        var budgetCollection = input[2] as BudgetCollection;

        if (this.filter is null
            || this.filter.Cleared
            || this.filter.BeginDate is null
            || this.filter.BeginDate == DateTime.MinValue
            || this.filter.EndDate is null
            || this.filter.EndDate.Value == DateTime.MinValue
            || this.bucketRepository is null)
        {
            Enabled = false;
            ToolTip = "The global filter dates are not set.";
            return;
        }

        if (budgetCollection is null)
        {
            Enabled = false;
            ToolTip = "The budget collection is empty or null.";
            return;
        }

        var budgetModel = budgetCollection.CurrentActiveBudget;
        if (budgetModel is null)
        {
            Enabled = false;
            ToolTip = "No active budget found.";
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

        if (budgetModel.BudgetCycle == BudgetCycle.Fortnightly)
        {
            Enabled = false;
            LargeNumber = string.Empty;
            ToolTip = "This widget only supports monthly budgets, current budget is fortnightly.";
            DetailedText = string.Empty;
            return;
        }

        this.diagLogger?.LogInfo(_ => $"{WidgetType.Name} Calculating Payment Plan for {Id}. From {this.filter.BeginDate} to {this.filter.EndDate}");
        var currentDate = CalculateStartDate(StartPaymentDate, this.filter.BeginDate.Value);
        var content = new StringBuilder();
        // Ignore start date in filter and force it to be one month prior to end date in filter.
        var currentMonthTally = new NextOccurance
        {
            StartDate = this.filter.EndDate.Value.AddDays(1).AddMonths(-1),
            EndDate = this.filter.EndDate.Value
        };
        var alert = false;
        NextOccurance? firstOccurance = null;
        do
        {
            if (currentDate.Date <= currentMonthTally.EndDate)
            {
                currentMonthTally.Tally(currentDate.Date.Day);
            }
            else
            {
                var tally4Log = currentMonthTally;
                this.diagLogger?.LogInfo(_ => $"    {tally4Log.StartDate:MMMM} {tally4Log.ConcatDates()}");
                if (AbnormalNumberOfPayments(currentMonthTally.Dates.Count))
                {
                    firstOccurance ??= currentMonthTally;

                    content.AppendFormat(CultureInfo.CurrentCulture, "{0:MMMM}, ", currentMonthTally.StartDate);
                    if (currentMonthTally.EndDate == this.filter.EndDate.Value || currentMonthTally.EndDate == this.filter.EndDate.Value.AddMonths(1))
                    {
                        // Is current or next month, so signal alert status
                        alert = true;
                        this.diagLogger?.LogInfo(_ => "    ***** ALERT *****");
                    }
                }

                currentMonthTally = currentMonthTally.NextMonth(currentDate.Date.Day);
            }

            currentDate = CalculateNextPaymentDate(currentDate);
        } while (currentDate.Date <= this.filter.EndDate.Value.AddYears(1));

        ColourStyleName = alert ? WidgetWarningStyle : WidgetStandardStyle;
        DetailedText = $"Monitoring {Frequency} {BucketCode} bucket. {content}";
        if (firstOccurance is null)
        {
            LargeNumber = string.Empty;
            ToolTip = ToolTipPrefix;
        }
        else
        {
            LargeNumber = firstOccurance.StartDate.ToString("MMMM");
            ToolTip = ToolTipPrefix + $"{firstOccurance.StartDate:d-MMM} to the {firstOccurance.EndDate:d-MMM} has payments on {firstOccurance.ConcatDates()}";
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
        if (this.filter!.BeginDate is not null)
        {
            var holidays = NewZealandPublicHolidays.CalculateHolidays(this.filter.BeginDate.Value, this.filter.BeginDate.Value.AddYears(1)).ToList();
            while (holidays.Contains(proposedDate.Date))
            {
                proposedDate.Date = proposedDate.Date.AddDays(1);
                proposedDate.Date = proposedDate.Date.FindNextWeekday();
            }
        }

        if (proposedDate.Date != proposedDate.ScheduledDate)
        {
            this.diagLogger?.LogInfo(_ => $"    {proposedDate.ScheduledDate} is a holiday, moved to {proposedDate.Date}");
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

        this.diagLogger?.LogInfo(_ => $"   Payment Start Date: {proposed.Date} ({proposed.ScheduledDate})");
        return proposed;
    }

    private class NextOccurance
    {
        public List<int> Dates { get; } = new();
        public DateTime EndDate { get; init; }
        public DateTime StartDate { get; init; }

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

    private record PaymentDate(DateTime Date)
    {
        public DateTime Date { get; set; } = Date;
        public DateTime ScheduledDate { get; set; } = Date;
    }
}
