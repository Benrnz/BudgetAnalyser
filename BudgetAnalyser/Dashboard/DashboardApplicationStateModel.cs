using System.Collections.Generic;

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
        public List<WidgetState> WidgetStates { get; set; }
    }
}
