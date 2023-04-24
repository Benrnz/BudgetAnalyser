using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A widget that monitors a bucket and tracks total spent for the month against funds available from the current
///     ledger book.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.RemainingBudgetBucketWidget" />
/// <seealso cref="BudgetAnalyser.Engine.Widgets.IUserDefinedWidget" />
public sealed class BudgetBucketMonitorWidget : RemainingBudgetBucketWidget, IUserDefinedWidget
{
    private readonly string disabledToolTip;
    private string doNotUseId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BudgetBucketMonitorWidget" /> class.
    /// </summary>
    public BudgetBucketMonitorWidget()
    {
        this.disabledToolTip = "Either a Statement or Budget are not present, or the Bucket Code is not valid, or the filtered date range doesn't match the budget period remaining budget cannot be calculated.";
    }

    /// <summary>
    ///     Gets or sets a unique identifier for this widget. This is the Bucket Code in this case.
    /// </summary>
    public string Id
    {
        get => this.doNotUseId;
        set
        {
            this.doNotUseId = value;
            OnPropertyChanged();
            BucketCode = Id;
        }
    }

    /// <summary>
    ///     Gets the type of the widget. In this case same as GetType().Name
    /// </summary>
    public Type WidgetType => GetType();

    /// <summary>
    ///     Initialises the widget and optionally offers it some state and a logger.
    /// </summary>
    public void Initialise(MultiInstanceWidgetState state, ILogger logger)
    {
    }

    /// <summary>
    ///     Updates the widget values with updated input.
    /// </summary>
    public override void Update(params object[] input)
    {
        base.Update(input);
        DetailedText = BucketCode;
        if (!Enabled)
        {
            ToolTip = this.disabledToolTip;
        }
    }
}