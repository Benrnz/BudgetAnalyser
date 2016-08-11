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
        ///     Create a new widget with the given parameters. This is used to instantiate the <see cref="IUserDefinedWidget" />s.
        ///     These can only be created after receiving the application state.
        /// </summary>
        /// <param name="fullName">The full type name of the widget type.</param>
        /// <param name="bucketCode">A unique identifier for the instance</param>
        IUserDefinedWidget Create(string fullName, string bucketCode);

        /// <summary>
        ///     Arranges the widgets into groups for display in the UI.
        /// </summary>
        /// <param name="storedStates">The stored states.</param>
        IEnumerable<WidgetGroup> PrepareWidgets([CanBeNull] IEnumerable<WidgetPersistentState> storedStates);

        /// <summary>
        ///     Removes the specified widget.
        /// </summary>
        void Remove(IUserDefinedWidget widget);
    }
}