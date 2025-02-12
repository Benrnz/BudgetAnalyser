using System.Collections.ObjectModel;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     Surfaces all Dashboard functionality.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Services.IServiceFoundation" />
public interface IDashboardService : IServiceFoundation
{
    /// <summary>
    ///     Creates a new bucket monitor widget and adds it to the tracked widgetGroups collection. Duplicates are not allowed in the collection and will not be added.
    /// </summary>
    /// <param name="bucketCode">The bucket code to create a new monitor widget for.</param>
    /// <returns>Will return a reference to the newly created widget, or null if the widget was not created because a duplicate already exists.</returns>
    Widget? CreateNewBucketMonitorWidget(string bucketCode);

    /// <summary>
    ///     Creates the new fixed budget monitor widget. Also creates all supporting background infrastructure to support the project including a subclass of Surplus.
    ///     Returns null is the bucket code already exists.
    /// </summary>
    /// <param name="bucketCode">The code to use for a <see cref="BudgetBucket" /> bucket code. This will be a bucket that inherits from Surplus.</param>
    /// <param name="description">The description.</param>
    /// <param name="fixedBudgetAmount">The fixed budget amount.</param>
    Widget? CreateNewFixedBudgetMonitorWidget(string bucketCode, string description, decimal fixedBudgetAmount);

    /// <summary>
    ///     Creates the new surprise payment monitor widget. This is a widget that shows which months require extra payments because four weeks do not perfectly divide into every month.
    ///     Returns null if the bucket code already has a Surprise Payment Monitor widget.
    /// </summary>
    Widget? CreateNewSurprisePaymentMonitorWidget(string bucketCode, DateTime paymentDate, WeeklyOrFortnightly frequency);

    /// <summary>
    ///     Removes a multi-instance widget from the widget groups.
    /// </summary>
    /// <param name="widgetToRemove">The widget to remove.</param>
    void RemoveUserDefinedWidget(IUserDefinedWidget widgetToRemove);

    /// <summary>
    ///     Makes all widgets visible.
    /// </summary>
    void ShowAllWidgets();

    /// <summary>
    ///     Initialises and returns the widget groups to view in the UI. This must be called first before other methods of this service can be used.
    ///     The collection of widget groups is cached inside the service for use by the other methods.
    /// </summary>
    /// <param name="storedState">The persisted widget application state data object.</param>
    ObservableCollection<WidgetGroup> WidgetsToDisplay(WidgetsApplicationState storedState);
}
