using System.Collections.ObjectModel;
using System.Diagnostics;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Widgets;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     A service class to retrieve and prepare the Widgets and arrange them in a grouped fashion for display in the UI.
/// </summary>
[AutoRegisterWithIoC]
[UsedImplicitly] // Used by IoC
internal class WidgetService : IWidgetService
{
    private readonly IBudgetBucketRepository bucketRepository;

    private readonly SortedList<string, Widget> cachedWidgets = new();

    // TODO private IApplicationDatabaseService? dbService;
    private readonly ILogger logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WidgetService" /> class.
    /// </summary>
    public WidgetService(IBudgetBucketRepository bucketRepository, ILogger logger)
    {
        this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Initialise(IEnumerable<Widget> widgetsFromPersistence)
    {
        if (widgetsFromPersistence is null)
        {
            throw new ArgumentNullException(nameof(widgetsFromPersistence));
        }

        foreach (var widget in widgetsFromPersistence)
        {
            string key;
            if (widget is IUserDefinedWidget userDefinedWidget)
            {
                key = BuildMultiUseWidgetKey(userDefinedWidget);
            }
            else
            {
                key = widget.Category + widget.Name;
            }

            this.cachedWidgets.Add(key, widget);
        }
    }

    /// <summary>
    ///     Removes the specified widget.
    /// </summary>
    public void RemoveUserDefinedWidget(IUserDefinedWidget widget)
    {
        if (widget is FixedBudgetMonitorWidget fixedProjectWidget)
        {
            // Reassign transactions to Surplus
            if (this.bucketRepository.GetByCode(fixedProjectWidget.BucketCode) is not FixedBudgetProjectBucket projectBucket)
            {
                throw new InvalidOperationException("The fixed project bucket provided doesn't actually appear to be a Fixed Budget Project Bucket");
            }

            projectBucket.Active = false;
            fixedProjectWidget.Visibility = false;
            // TODO this.dbService.NotifyOfChange(ApplicationDataType.Budget);
            return;
        }

        this.cachedWidgets.Remove(BuildMultiUseWidgetKey(widget));
    }

    public ObservableCollection<WidgetGroup> ArrangeWidgetsForDisplay()
    {
        var widgetGroups = this.cachedWidgets.Values
            .GroupBy(w => w.Category)
            .Select(group => new WidgetGroup { Heading = group.Key, Widgets = new ObservableCollection<Widget>(group.OrderBy(w => w.Sequence)), Sequence = WidgetGroup.GroupSequence[group.Key] });
        return new ObservableCollection<WidgetGroup>(widgetGroups);
    }

    public IUserDefinedWidget CreateFixedBudgetMonitorWidget(string bucketCode, string description, decimal fixedBudgetAmount)
    {
        var bucket = this.bucketRepository.CreateNewFixedBudgetProject(bucketCode, description, fixedBudgetAmount);
        // TODO this.dbService.NotifyOfChange(ApplicationDataType.Budget);
        return CreateUserDefinedWidget(description, bucket.Code);
    }

    /// <summary>
    ///     Create a new widget with the given parameters. This is used to instantiate the <see cref="IUserDefinedWidget" />s.
    ///     These can only be created after receiving the application state.
    /// </summary>
    /// <param name="fullName">The full type name of the widget type.</param>
    /// <param name="bucketCode">A unique identifier for the instance</param>
    public IUserDefinedWidget CreateUserDefinedWidget(string fullName, string bucketCode)
    {
        if (this.bucketRepository.GetByCode(bucketCode) is null)
        {
            throw new ArgumentException($"No Bucket with code {bucketCode} exists", nameof(bucketCode));
        }

        var type = Type.GetType(fullName) ?? throw new DataFormatException($"The widget type specified {fullName} is not found in any known type library.");
        if (!typeof(IUserDefinedWidget).IsAssignableFrom(type))
        {
            throw new DataFormatException($"The widget type specified {fullName} is not a IUserDefinedWidget");
        }

        var widget = Activator.CreateInstance(type) as IUserDefinedWidget;
        Debug.Assert(widget is not null);
        widget.Id = bucketCode;
        var key = BuildMultiUseWidgetKey(widget);

        if (this.cachedWidgets.ContainsKey(key))
        {
            throw new ArgumentException("A widget with this key already exists.", nameof(bucketCode));
        }

        var baseWidget = (Widget)widget;
        this.cachedWidgets.Add(key, baseWidget);
        return widget;
    }

    private static string BuildMultiUseWidgetKey(IUserDefinedWidget widget)
    {
        var baseWidget = (Widget)widget;
        return baseWidget.Category + baseWidget.Name + widget.Id;
    }
}
