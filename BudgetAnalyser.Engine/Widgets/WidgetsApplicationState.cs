using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A state persistence Dto for widget data.
    /// </summary>
    /// <seealso cref="IPersistentApplicationStateObject" />
    public class WidgetsApplicationState : IPersistentApplicationStateObject
    {
        /// <summary>
        ///     Gets the order in which this object should be loaded.
        /// </summary>
        public int LoadSequence => 100;

        /// <summary>
        ///     Gets or sets the widget states.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists",
            Justification = "Required for serialisation")]
        public List<WidgetPersistentState> WidgetStates { get; set; }
    }
}