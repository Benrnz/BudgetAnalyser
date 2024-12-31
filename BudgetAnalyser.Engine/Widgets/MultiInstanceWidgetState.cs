using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A Dto to store persistent state for multi-instance widgets.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.WidgetPersistentState" />
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi",
        Justification = "Preferred spelling")]
    public class MultiInstanceWidgetState : WidgetPersistentState
    {
        /// <summary>
        ///     Gets or sets the unique identifier.
        /// </summary>
        public string Id { get; set; }
    }
}
