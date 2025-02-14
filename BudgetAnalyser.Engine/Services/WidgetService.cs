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

    private readonly ILogger logger;
    private readonly IMonitorableDependencies monitoringServices;
    private readonly CancellationTokenSource scheduledTaskCancellation = new();

    public WidgetService(IBudgetBucketRepository bucketRepository, IMonitorableDependencies monitorableDependencies, ILogger logger)
    {
        this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.monitoringServices = monitorableDependencies ?? throw new ArgumentNullException(nameof(monitorableDependencies));
        this.monitoringServices.DependencyChanged += OnMonitoringServicesDependencyChanged;
    }

    /// <inheritdoc />
    public void Initialise(IEnumerable<Widget> widgetsFromPersistence)
    {
        if (widgetsFromPersistence is null)
        {
            throw new ArgumentNullException(nameof(widgetsFromPersistence));
        }

        var widgets = widgetsFromPersistence.ToList();
        if (widgets.None())
        {
            throw new ArgumentException("The widgets collection should never be empty, it must contain a default set of widgets.", nameof(widgetsFromPersistence));
        }

        foreach (var widget in widgets)
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

        UpdateAllWidgets();

        // Schedule widgets that want to do regular timed updates.
        var token = this.scheduledTaskCancellation.Token;
        foreach (var widget in this.cachedWidgets.Values.Where(w => w.RecommendedTimeIntervalUpdate is not null))
        {
            // Run the scheduling on a different thread.
            _ = Task.Run(() => ScheduledWidgetUpdate(widget, token), token);
        }
    }

    /// <inheritdoc />
    public void RemoveUserDefinedWidget(IUserDefinedWidget widget)
    {
        if (widget is FixedBudgetMonitorWidget fixedProjectWidget)
        {
            // Reassign transactions to Surplus
            if (this.bucketRepository.GetByCode(fixedProjectWidget.BucketCode) is not FixedBudgetProjectBucket projectBucket)
            {
                throw new InvalidOperationException("The widget provided doesn't appear to be a Fixed Budget Project Bucket");
            }

            projectBucket.Active = false;
            fixedProjectWidget.Visibility = false;
            return;
        }

        this.cachedWidgets.Remove(BuildMultiUseWidgetKey(widget));
    }

    /// <inheritdoc />
    public Widget? CreateNewSurprisePaymentWidget(string bucketCode, DateTime paymentDate, WeeklyOrFortnightly frequency)
    {
        var widget = CreateUserDefinedWidget(typeof(SurprisePaymentWidget).FullName!, bucketCode);
        if (widget is null)
        {
            return null;
        }

        var surpriseWidget = (SurprisePaymentWidget)widget;
        surpriseWidget.StartPaymentDate = paymentDate;
        surpriseWidget.Frequency = frequency;
        return surpriseWidget;
    }

    /// <inheritdoc />
    public ObservableCollection<WidgetGroup> ArrangeWidgetsForDisplay()
    {
        var widgetGroups = this.cachedWidgets.Values
            .GroupBy(w => w.Category)
            .Select(group => new WidgetGroup { Heading = group.Key, Widgets = new ObservableCollection<Widget>(group.OrderBy(w => w.Sequence)), Sequence = WidgetGroup.GroupSequence[group.Key] })
            .OrderBy(group => group.Sequence);
        return new ObservableCollection<WidgetGroup>(widgetGroups);
    }

    /// <inheritdoc />
    public IUserDefinedWidget? CreateFixedBudgetMonitorWidget(string bucketCode, string description, decimal fixedBudgetAmount)
    {
        var bucket = this.bucketRepository.CreateNewFixedBudgetProject(bucketCode, description, fixedBudgetAmount);
        if (bucket is null)
        {
            return null;
        }

        return CreateUserDefinedWidget(typeof(FixedBudgetMonitorWidget).FullName!, bucket.Code);
    }

    /// <inheritdoc />
    public IUserDefinedWidget? CreateUserDefinedWidget(string fullName, string bucketCode)
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
            return null;
        }

        var baseWidget = (Widget)widget;
        this.cachedWidgets.Add(key, baseWidget);
        return widget;
    }

    public void CancelScheduledUpdates()
    {
        this.scheduledTaskCancellation.Cancel();
    }

    /// <inheritdoc />
    public void UpdateWidgetData(Widget widget)
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

    private static string BuildMultiUseWidgetKey(IUserDefinedWidget widget)
    {
        var baseWidget = (Widget)widget;
        return baseWidget.Category + baseWidget.Name + widget.Id;
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
            UpdateWidgetData(widget);
        }
        // ReSharper disable once FunctionNeverReturns - intentional timer tick infinite loop
    }

    private void UpdateAllWidgets(params Type[] filterDependencyTypes)
    {
        if (this.cachedWidgets.None())
        {
            // Widget Groups have not yet been initialised and persistent state has not yet been loaded.
            return;
        }

        if (filterDependencyTypes.Length > 0)
        {
            // targeted update - Eg. If only the LedgerBook has changed only Widgets that depend on LedgerBook should be updated.
            this.cachedWidgets.Values
                .Where(w => w.Dependencies.Any(filterDependencyTypes.Contains))
                .ToList()
                .ForEach(UpdateWidgetData);
        }
        else
        {
            // update all
            this.cachedWidgets.Values.ToList().ForEach(UpdateWidgetData);
        }
    }
}
