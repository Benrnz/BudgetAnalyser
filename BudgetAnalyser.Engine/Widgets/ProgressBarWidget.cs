using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A base class for any widget that wants to show a progress bar to represent a comparison bar graph.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
public abstract class ProgressBarWidget : Widget
{
    private bool doNotUseEnabled;
    private double doNotUseMaximum;
    private double doNotUseMinimum;
    private bool doNotUseProgressBarVisibility;
    private double doNotUseValue;

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="Widget" /> is enabled, showing data, and clickable.
    /// </summary>
    public override bool Enabled
    {
        get => this.doNotUseEnabled;
        protected set
        {
            if (value == this.doNotUseEnabled)
            {
                return;
            }

            this.doNotUseEnabled = value;
            if (!this.doNotUseEnabled)
            {
                Value = 0;
                ProgressBarVisibility = false;
            }
            else
            {
                ProgressBarVisibility = true;
            }

            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the maximum value.
    /// </summary>
    public double Maximum
    {
        get => this.doNotUseMaximum;
        protected set
        {
            if (Math.Abs(value - this.doNotUseMaximum) < 0.001)
            {
                return;
            }

            this.doNotUseMaximum = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the minimum value.
    /// </summary>
    public double Minimum
    {
        get => this.doNotUseMinimum;
        protected set
        {
            if (Math.Abs(value - this.doNotUseMinimum) < 0.001)
            {
                return;
            }

            this.doNotUseMinimum = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether progress bar is visible.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for Data binding")]
    public bool ProgressBarVisibility
    {
        get => this.doNotUseProgressBarVisibility;
        protected set
        {
            this.doNotUseProgressBarVisibility = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the graph value.
    /// </summary>
    public double Value
    {
        get => this.doNotUseValue;
        set
        {
            if (Math.Abs(value - this.doNotUseValue) < 0.001)
            {
                return;
            }

            this.doNotUseValue = value;
            OnPropertyChanged();
        }
    }
}
