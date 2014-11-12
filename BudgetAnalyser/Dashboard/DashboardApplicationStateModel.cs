using System.Collections.Generic;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Dashboard
{
    /// <summary>
    /// The stored application state for the Dashboard view.
    /// This is saved when the application exits.
    /// </summary>
    public class DashboardApplicationStateModel
    {
        /// <summary>
        /// The stored application state for widgets.  
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<WidgetPersistentState> WidgetStates { get; set; }
    }
}
