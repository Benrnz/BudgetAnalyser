using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     The stored application state for the Main view.
    ///     This is saved when the application exits.
    /// </summary>
    public class MainApplicationStateModel
    {
        /// <summary>
        ///     Gets or sets a string that indicates where the budget analyser stores its data.
        /// </summary>
        public string BudgetAnalyserDataStorage { get; set; }

        /// <summary>
        ///     The stored application state for widgets.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<WidgetPersistentState> WidgetStates { get; set; }
    }
}