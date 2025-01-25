using System.Collections.ObjectModel;
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
    private static readonly Dictionary<string, int> GroupSequence = new()
    {
        { WidgetGroup.OverviewSectionName, 1 }, { WidgetGroup.GlobalFilterSectionName, 2 }, { WidgetGroup.PeriodicTrackingSectionName, 3 }, { WidgetGroup.ProjectsSectionName, 4 }
    };

    private readonly IBudgetBucketRepository bucketRepository;
    private readonly IApplicationDatabaseService dbService;
    private readonly ILogger logger;
    private readonly IWidgetRepository widgetRepo;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WidgetService" /> class.
    /// </summary>
    public WidgetService(IApplicationDatabaseService dbService, IBudgetBucketRepository bucketRepository, IWidgetRepository widgetRepo, ILogger logger)
    {
        this.dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
        this.widgetRepo = widgetRepo ?? throw new ArgumentNullException(nameof(widgetRepo));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Prepares the widgets.
    /// </summary>
    /// <param name="storedStates">The stored application state for widgets.</param>
    public IEnumerable<WidgetGroup> PrepareWidgets(IEnumerable<WidgetPersistentState>? storedStates)
    {
        if (storedStates is not null)
        {
            var widgets = this.widgetRepo.GetAll().ToList();
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

        return this.widgetRepo.GetAll()
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

        this.widgetRepo.Remove(widget);
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
        return this.widgetRepo.Create(fullName, bucket.Code);
    }

    private void CreateMultiInstanceWidget(MultiInstanceWidgetState multiInstanceState)
    {
        // MultiInstance widgets need to be created at this point.  The App State data is required to create them.
        var newIdWidget = this.widgetRepo.Create(multiInstanceState.WidgetType, multiInstanceState.Id);
        newIdWidget.Visibility = multiInstanceState.Visible;
        newIdWidget.Initialise(multiInstanceState, this.logger);
    }
}
