using System.Globalization;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A fixed budget project monitor widget.  Used to monitor spend for a <see cref="FixedBudgetProjectBucket" />.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.ProgressBarWidget" />
/// <seealso cref="BudgetAnalyser.Engine.Widgets.IUserDefinedWidget" />
public sealed class FixedBudgetMonitorWidget : ProgressBarWidget, IUserDefinedWidget
{
    private const string DisabledToolTip = "No Transactions are loaded, or bucket doesn't exist.";
    private const string RemainingBudgetToolTip = "{0} Remaining budget for this project: {1:C}. Total Spend {2:C}";
    private const string StandardStyle = "WidgetStandardStyle1";
    private IBudgetBucketRepository? bucketRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FixedBudgetMonitorWidget" /> class.
    /// </summary>
    public FixedBudgetMonitorWidget()
    {
        Category = WidgetGroup.ProjectsSectionName;
        Dependencies = [typeof(TransactionsListModel), typeof(IBudgetBucketRepository)];
        RecommendedTimeIntervalUpdate = TimeSpan.FromHours(6);
        Enabled = false;
    }

    /// <summary>
    ///     Gets or sets the bucket code.
    /// </summary>
    public string BucketCode
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            DetailedText = BucketCode;
        }
    } = NotSet;

    /// <summary>
    ///     Gets the transactions model.
    /// </summary>
    public TransactionsListModel? TransactionsModel { get; private set; }

    /// <summary>
    ///     Gets the type of the widget. Optionally allows the implementation to override the widget type description used in
    ///     the user interface.
    /// </summary>
    public Type WidgetType => GetType();

    /// <summary>
    ///     Gets or sets a unique identifier for the widget. This is required for persistence purposes.
    /// </summary>
    public string Id
    {
        get => BucketCode;
        set
        {
            BucketCode = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    /// <exception cref="System.ArgumentNullException"></exception>
    public override void Update(params object[] input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (!ValidateUpdateInput(input))
        {
            Disable();
            return;
        }

        TransactionsModel = input[0] as TransactionsListModel;
        this.bucketRepository = (IBudgetBucketRepository)input[1];

        if (!this.bucketRepository.IsValidCode(BucketCode) || TransactionsModel is null)
        {
            Disable();
            return;
        }

        Enabled = true;
        var bucket = this.bucketRepository.GetByCode(BucketCode) as FixedBudgetProjectBucket ??
                     throw new InvalidCastException($"The provided bucket code '{BucketCode}' is not a Fixed Budget Project Bucket.");
        var totalBudget = bucket.FixedBudgetAmount;
        Maximum = Convert.ToDouble(totalBudget);

        // Debit transactions are negative so normally the total spend will be a negative number.
        var totalSpend =
            TransactionsModel.AllTransactions.Where(t => t.BudgetBucket is not null && t.BudgetBucket.Code == BucketCode)
                .Sum(t => t.Amount);
        var remainingBudget = totalBudget + totalSpend;

        Value = Convert.ToDouble(remainingBudget);
        ToolTip = string.Format(CultureInfo.CurrentCulture, RemainingBudgetToolTip, bucket.Description,
            remainingBudget, totalSpend);
        DetailedText = string.Format(CultureInfo.CurrentCulture, "{0} Project", bucket.SubCode);

        ColourStyleName = remainingBudget < 0.1M * totalBudget ? WidgetWarningStyle : StandardStyle;
    }

    private void Disable()
    {
        ToolTip = DisabledToolTip;
        Enabled = false;
    }
}
