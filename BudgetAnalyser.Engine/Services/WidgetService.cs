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
    // TODO move this to WidgetGroup
    private static readonly Dictionary<string, int> GroupSequence = new()
    {
        { WidgetGroup.OverviewSectionName, 1 }, { WidgetGroup.GlobalFilterSectionName, 2 }, { WidgetGroup.PeriodicTrackingSectionName, 3 }, { WidgetGroup.ProjectsSectionName, 4 }
    };

    private readonly IBudgetBucketRepository bucketRepository;

    private readonly SortedList<string, Widget> cachedWidgets = new();
    private readonly IStandardWidgetCatalog catalog;
    private readonly IApplicationDatabaseService dbService;
    private readonly ILogger logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WidgetService" /> class.
    /// </summary>
    public WidgetService(IApplicationDatabaseService dbService, IBudgetBucketRepository bucketRepository, ILogger logger, IStandardWidgetCatalog catalog)
    {
        this.dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    /// <summary>
    ///     Prepares the widgets.
    /// </summary>
    /// <param name="storedStates">The stored application state for widgets.</param>
    public IEnumerable<WidgetGroup> PrepareWidgets(IEnumerable<WidgetPersistentState>? storedStates)
    {
        if (storedStates is not null)
        {
            var widgets = GetAll();
            foreach (var widgetState in storedStates)
            {
                var stateClone = widgetState;
                if (widgetState is MultiInstanceWidgetState multiInstanceState)
                {
                    CreateMultiInstanceWidget(multiInstanceState);
                }
                else
                {
                    // Ordinary widgets will already exist in the repository as they are single instance per class.
                    var typedWidget = widgets.FirstOrDefault(w => w.GetType().FullName == stateClone.WidgetType);
                    if (typedWidget is not null)
                    {
                        typedWidget.Visibility = widgetState.Visible;
                    }
                }
            }
        }

        return GetAll()
            .GroupBy(w => w.Category)
            .Select(
                group => new WidgetGroup { Heading = group.Key, Widgets = new ObservableCollection<Widget>(group.OrderBy(w => w.Sequence)), Sequence = GroupSequence[group.Key] })
            .OrderBy(g => g.Sequence).ThenBy(g => g.Heading);
    }

    /// <summary>
    ///     Removes the specified widget.
    /// </summary>
    public void Remove(IUserDefinedWidget widget)
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
            this.dbService.NotifyOfChange(ApplicationDataType.Budget);
            return;
        }

        this.cachedWidgets.Remove(BuildMultiUseWidgetKey(widget));
    }

    public IUserDefinedWidget CreateFixedBudgetMonitorWidget(string bucketCode, string description, decimal fixedBudgetAmount)
    {
        var bucket = this.bucketRepository.CreateNewFixedBudgetProject(bucketCode, description, fixedBudgetAmount);
        this.dbService.NotifyOfChange(ApplicationDataType.Budget);
        return Create(description, bucket.Code);
    }

    /// <summary>
    ///     Create a new widget with the given parameters. This is used to instantiate the <see cref="IUserDefinedWidget" />s.
    ///     These can only be created after receiving the application state.
    /// </summary>
    /// <param name="fullName">The full type name of the widget type.</param>
    /// <param name="bucketCode">A unique identifier for the instance</param>
    public IUserDefinedWidget Create(string fullName, string bucketCode)
    {
        var bucket = this.bucketRepository.GetByCode(bucketCode) ?? throw new ArgumentException($"No Bucket with code {bucketCode} exists", nameof(bucketCode));

        var type = Type.GetType(fullName) ?? throw new DataFormatException($"The widget type specified {fullName} is not found in any known type library.");
        if (!typeof(IUserDefinedWidget).IsAssignableFrom(type))
        {
            throw new DataFormatException($"The widget type specified {fullName} is not a IUserDefinedWidget");
        }

        var widget = Activator.CreateInstance(type) as IUserDefinedWidget;
        Debug.Assert(widget is not null);
        widget.Id = bucket.Code;
        var key = BuildMultiUseWidgetKey(widget);

        if (this.cachedWidgets.ContainsKey(key))
        {
            throw new ArgumentException("A widget with this key already exists.", nameof(bucket.Code));
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

    private void CreateMultiInstanceWidget(MultiInstanceWidgetState multiInstanceState)
    {
        // MultiInstance widgets need to be created at this point.  The App State data is required to create them.
        var newIdWidget = Create(multiInstanceState.WidgetType, multiInstanceState.Id);
        newIdWidget.Visibility = multiInstanceState.Visible;
        newIdWidget.Initialise(multiInstanceState, this.logger);
    }

    private IList<Widget> GetAll()
    {
        if (this.cachedWidgets.None())
        {
            foreach (var widget in this.catalog.GetAll())
            {
                this.cachedWidgets.Add(widget.Category + widget.Name, widget);
            }
        }

        return this.cachedWidgets.Values;
    }
}
