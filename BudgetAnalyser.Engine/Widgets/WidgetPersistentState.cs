namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Application state stored for a single widget.
    /// </summary>
    public class WidgetPersistentState
    {
        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="WidgetPersistentState" /> is visible. Ie: Has the user
        ///     opted to hide this widget.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        ///     Gets or sets the type of the widget.
        /// </summary>
        public string WidgetType { get; set; }

        // TODO This state persistence is being misused currently.  Custom monitor widgets should be saved with model data not state persistence.
    }
}