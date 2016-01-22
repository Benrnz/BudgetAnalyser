using System.Collections.Generic;
using BudgetAnalyser.Engine.Widgets;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service class to retrieve and prepare the Widgets and arrange them in a grouped fashion for display in the UI.
    /// </summary>
    public interface IWidgetService
    {
        /// <summary>
        ///     Arranges the widgets into groups for display in the UI.
        /// </summary>
        /// <param name="storedStates">The stored states.</param>
        IEnumerable<WidgetGroup> PrepareWidgets([CanBeNull] IEnumerable<WidgetPersistentState> storedStates);
    }
}