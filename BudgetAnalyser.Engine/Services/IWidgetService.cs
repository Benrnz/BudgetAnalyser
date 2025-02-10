using System.Collections.ObjectModel;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     A service class to retrieve and prepare the Widgets and arrange them in a grouped fashion for display in the UI.
/// </summary>
public interface IWidgetService
{
    /// <summary>
    ///     Arranges the widgets into groups for display in the UI.
    /// </summary>
    /// <returns></returns>
    ObservableCollection<WidgetGroup> ArrangeWidgetsForDisplay();

    IUserDefinedWidget CreateFixedBudgetMonitorWidget(string bucketCode, string description, decimal fixedBudgetAmount);

    /// <summary>
    ///     Create a new widget with the given parameters. This is used to instantiate the <see cref="IUserDefinedWidget" />s.
    ///     These can only be created after receiving the application state.
    /// </summary>
    /// <param name="fullName">The full type name of the widget type.</param>
    /// <param name="bucketCode">A unique identifier for the instance</param>
    IUserDefinedWidget CreateUserDefinedWidget(string fullName, string bucketCode);

    /// <summary>
    ///     Initialise the service with widgets freshly loaded from persistence. This must be called first before other methods.
    /// </summary>
    void Initialise(IEnumerable<Widget> widgetsFromPersistence);

    /// <summary>
    ///     Removes the specified widget.
    /// </summary>
    void RemoveUserDefinedWidget(IUserDefinedWidget widget);
}
