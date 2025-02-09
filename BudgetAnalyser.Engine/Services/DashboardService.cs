using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Engine.Services;

[AutoRegisterWithIoC(SingleInstance = true)]
// ReSharper disable once UnusedType.Global // Instantiated by IoC
internal class DashboardService : IDashboardService, ISupportsModelPersistence
{
    private readonly ILogger logger;
    private readonly MonitorableDependencies monitoringServices;
    private readonly IWidgetRepository widgetRepo;
    private readonly IWidgetService widgetService;
    private readonly CancellationTokenSource scheduledTaskCancellation = new();
    private ObservableCollection<WidgetGroup> widgetGroups = new();

    public DashboardService(
        IWidgetService widgetService,
        ILogger logger,
        MonitorableDependencies monitorableDependencies,
        IWidgetRepository widgetRepo)
    {
        this.widgetService = widgetService ?? throw new ArgumentNullException(nameof(widgetService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.monitoringServices = monitorableDependencies ?? throw new ArgumentNullException(nameof(monitorableDependencies));
        this.widgetRepo = widgetRepo ?? throw new ArgumentNullException(nameof(widgetRepo));
        this.monitoringServices.DependencyChanged += OnMonitoringServicesDependencyChanged;
    }

    /// <inheritdoc />
    public Widget? CreateNewBucketMonitorWidget(string bucketCode)
    {
        if (this.widgetGroups.SelectMany(group => group.Widgets)
            .OfType<BudgetBucketMonitorWidget>()
            .Any(w => w.BucketCode == bucketCode))
        {
            // Bucket code already exists - so already has a bucket monitor widget.
            return null;
        }

        var widget = this.widgetService.Create(typeof(BudgetBucketMonitorWidget).FullName!, bucketCode);
        return UpdateWidgetCollectionWithNewAddition((Widget)widget);
    }

    /// <inheritdoc />
    public void CreateNewFixedBudgetMonitorWidget(string bucketCode, string description, decimal fixedBudgetAmount)
    {
        if (string.IsNullOrWhiteSpace(bucketCode))
        {
            throw new ArgumentNullException(nameof(bucketCode));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentNullException(nameof(description));
        }

        if (fixedBudgetAmount <= 0)
        {
            throw new ArgumentException("Fixed Budget amount must be greater than zero.", nameof(fixedBudgetAmount));
        }

        var widget = this.widgetService.CreateFixedBudgetMonitorWidget(bucketCode, typeof(FixedBudgetMonitorWidget).FullName!, fixedBudgetAmount);
        UpdateWidgetCollectionWithNewAddition((Widget)widget);
    }

    /// <inheritdoc />
    public void CreateNewSurprisePaymentMonitorWidget(string bucketCode, DateTime paymentDate, WeeklyOrFortnightly frequency)
    {
        if (string.IsNullOrWhiteSpace(bucketCode))
        {
            throw new ArgumentNullException(nameof(bucketCode));
        }

        if (paymentDate == DateTime.MinValue)
        {
            throw new ArgumentException("Payment date is not set.", nameof(paymentDate));
        }

        var widget = this.widgetService.Create(typeof(SurprisePaymentWidget).FullName!, bucketCode);
        var paymentWidget = (SurprisePaymentWidget)widget;
        paymentWidget.StartPaymentDate = paymentDate;
        paymentWidget.Frequency = frequency;
        UpdateWidgetCollectionWithNewAddition((Widget)widget);
    }

    /// <inheritdoc />
    public ObservableCollection<WidgetGroup> LoadPersistedStateData(WidgetsApplicationState storedState)
    {
        if (storedState is null)
        {
            throw new ArgumentNullException(nameof(storedState));
        }

        this.widgetGroups = new ObservableCollection<WidgetGroup>(this.widgetService.PrepareWidgets(storedState.WidgetStates));
        UpdateAllWidgets();
        foreach (var group in this.widgetGroups)
        {
            var token = this.scheduledTaskCancellation.Token;
            foreach (var widget in group.Widgets.Where(widget => widget.RecommendedTimeIntervalUpdate is not null))
            {
                // Run the scheduling on a different thread.
                Task.Run(() => ScheduledWidgetUpdate(widget, token), token);
            }
        }

        return this.widgetGroups;
    }

    /// <inheritdoc />
    public WidgetsApplicationState PreparePersistentStateData()
    {
        IEnumerable<WidgetGroup> widgetStates = this.widgetGroups.ToList();
        return new WidgetsApplicationState { WidgetStates = widgetStates.SelectMany(group => group.Widgets).Select(CreateWidgetState).ToList() };
    }

    /// <inheritdoc />
    public void RemoveUserDefinedWidget(IUserDefinedWidget widgetToRemove)
    {
        this.widgetService.Remove(widgetToRemove);

        var baseWidget = (Widget)widgetToRemove;
        var widgetGroup = this.widgetGroups.FirstOrDefault(group => group.Heading == baseWidget.Category);

        widgetGroup?.Widgets.Remove(baseWidget);
    }

    /// <inheritdoc />
    public void ShowAllWidgets()
    {
        this.widgetGroups
            .ToList()
            .ForEach(g => g.Widgets.ToList().ForEach(w => w.Visibility = true));
    }

    public ApplicationDataType DataType => ApplicationDataType.Widgets;

    public int LoadSequence => 99;

    public void Close()
    {
        this.scheduledTaskCancellation.Cancel();
    }

    public async Task CreateAsync(ApplicationDatabase applicationDatabase)
    {
        await this.widgetRepo.CreateNewAndSaveAsync(applicationDatabase.WidgetsCollectionStorageKey);
    }

    public Task LoadAsync(ApplicationDatabase applicationDatabase)
    {
        // TODO Do nothing at this stage
        return Task.CompletedTask;
    }

    public async Task SaveAsync(ApplicationDatabase applicationDatabase)
    {
        await this.widgetRepo.SaveAsync(this.widgetGroups.SelectMany(g => g.Widgets), applicationDatabase.WidgetsCollectionStorageKey, applicationDatabase.IsEncrypted);
    }

    public void SavePreview()
    {
    }

    public bool ValidateModel(StringBuilder messages)
    {
        return true;
    }

    private static WidgetPersistentState CreateWidgetState(Widget widget)
    {
        if (widget is IUserDefinedWidget multiInstanceWidget)
        {
            return multiInstanceWidget is not SurprisePaymentWidget surprisePaymentWidget
                ? new MultiInstanceWidgetState { Id = multiInstanceWidget.Id, Visible = multiInstanceWidget.Visibility, WidgetType = multiInstanceWidget.WidgetType.FullName! }
                : (WidgetPersistentState)new SurprisePaymentWidgetPersistentState
                {
                    Id = surprisePaymentWidget.Id,
                    Visible = surprisePaymentWidget.Visibility,
                    WidgetType = surprisePaymentWidget.WidgetType.FullName!,
                    PaymentStartDate = surprisePaymentWidget.StartPaymentDate,
                    Frequency = surprisePaymentWidget.Frequency
                };
        }

        return new WidgetPersistentState { Visible = widget.Visibility, WidgetType = widget.GetType().FullName! };
    }

    private void OnMonitoringServicesDependencyChanged(object? sender, DependencyChangedEventArgs? dependencyChangedEventArgs)
    {
        if (dependencyChangedEventArgs is null)
        {
            return;
        }

        UpdateAllWidgets(dependencyChangedEventArgs.DependencyType);
    }

    private async Task ScheduledWidgetUpdate(Widget widget, CancellationToken token)
    {
        Debug.Assert(widget.RecommendedTimeIntervalUpdate is not null);
        this.logger.LogInfo(_ => $"Scheduling \"{widget.Name}\" widget to update every {widget.RecommendedTimeIntervalUpdate.Value.TotalMinutes} minutes.");

        while (!token.IsCancellationRequested)
        {
            await Task.Delay(widget.RecommendedTimeIntervalUpdate.Value, token);
            this.logger.LogInfo(_ => $"Scheduled Update for \"{widget.Name}\" widget. Will run again after {widget.RecommendedTimeIntervalUpdate.Value.TotalMinutes} minutes.");
            UpdateWidget(widget);
        }
        // ReSharper disable once FunctionNeverReturns - intentional timer tick infinite loop
    }

    private void UpdateAllWidgets(params Type[] filterDependencyTypes)
    {
        if (this.widgetGroups.None())
        {
            // Widget Groups have not yet been initialised and persistent state has not yet been loaded.
            return;
        }

        if (filterDependencyTypes.Length > 0)
        {
            // targeted update
            this.widgetGroups.SelectMany(group => group.Widgets)
                .Where(w => w.Dependencies.Any(filterDependencyTypes.Contains))
                .ToList()
                .ForEach(UpdateWidget);
        }
        else
        {
            // update all
            this.widgetGroups.SelectMany(group => group.Widgets).ToList().ForEach(UpdateWidget);
        }
    }

    private void UpdateWidget(Widget widget)
    {
        if (widget.Dependencies.None())
        {
            widget.Update();
            return;
        }

        var parameters = new object[widget.Dependencies.Count()];
        var index = 0;
        foreach (var dependencyType in widget.Dependencies)
        {
            try
            {
                parameters[index++] = this.monitoringServices.RetrieveDependency(dependencyType);
            }
            catch (NotSupportedException ex)
            {
                // If you get an exception here first check the MonitorableDependencies.ctor method.
                throw new NotSupportedException($"The requested dependency {dependencyType.Name} for the widget {widget.Name} is not supported.", ex);
            }
        }

        widget.Update(parameters);
    }

    private Widget UpdateWidgetCollectionWithNewAddition(Widget baseWidget)
    {
        var widgetGroup = this.widgetGroups.FirstOrDefault(group => group.Heading == baseWidget.Category);
        if (widgetGroup is null)
        {
            widgetGroup = new WidgetGroup { Heading = baseWidget.Category, Widgets = new ObservableCollection<Widget>() };
            this.widgetGroups.Add(widgetGroup);
        }

        widgetGroup.Widgets.Add(baseWidget);
        UpdateAllWidgets();
        return baseWidget;
    }
}
