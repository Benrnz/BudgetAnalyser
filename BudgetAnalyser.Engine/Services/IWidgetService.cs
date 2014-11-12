using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    /// A service class to retrieve and prepare the Widgets and arrange them in a grouped fashion for display in the UI.
    /// </summary>
    public interface IWidgetService
    {
        IEnumerable<WidgetGroup> PrepareWidgets([CanBeNull] IEnumerable<WidgetPersistentState> storedStates);
    }
}