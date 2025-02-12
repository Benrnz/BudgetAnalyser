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

    /// <summary>
    ///     Cancels all scheduled widget data update tasks. Used when closing the application or file.
    /// </summary>
    void CancelScheduledUpdates();

    /// <summary>
    ///     Create a new Fixed Budget Monitor Widget with the provided parameters. Returns null if the specified bucket code already exists.
    /// </summary>
    IUserDefinedWidget? CreateFixedBudgetMonitorWidget(string bucketCode, string description, decimal fixedBudgetAmount);

    /// <summary>
    ///     Create a new Surprise Payment Widget with the provided parameters. Returns null if the specified bucket code already exists.
    /// </summary>
    Widget? CreateNewSurprisePaymentWidget(string bucketCode, DateTime paymentDate, WeeklyOrFortnightly frequency);

    /// <summary>
    ///     Create a new widget with the given parameters. This is used to instantiate the <see cref="IUserDefinedWidget" />s.
    ///     These can only be created after receiving the application state. Will return null if a widget already exists for the provided bucket code.
    /// </summary>
    /// <param name="fullName">The full type name of the widget type.</param>
    /// <param name="bucketCode">A unique identifier for the instance</param>
    IUserDefinedWidget? CreateUserDefinedWidget(string fullName, string bucketCode);

    /// <summary>
    ///     Initialise the service with widgets freshly loaded from persistence. This must be called first before other methods.
    /// </summary>
    void Initialise(IEnumerable<Widget> widgetsFromPersistence);

    /// <summary>
    ///     Removes the specified widget.
    /// </summary>
    void RemoveUserDefinedWidget(IUserDefinedWidget widget);

    /// <summary>
    ///     Force an update for the provided widget to refresh its data and update its values to display.
    /// </summary>
    void UpdateWidgetData(Widget widget);
}
