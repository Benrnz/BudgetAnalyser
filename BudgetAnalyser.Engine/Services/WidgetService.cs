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

    // TODO private IApplicationDatabaseService? dbService; // Used to signal changes have been made to widgets than need to be persisted.
    private readonly ILogger logger;

    public WidgetService(IBudgetBucketRepository bucketRepository, ILogger logger)
    {
        this.bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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
            .Select(group => new WidgetGroup { Heading = group.Key, Widgets = new ObservableCollection<Widget>(group.OrderBy(w => w.Sequence)), Sequence = WidgetGroup.GroupSequence[group.Key] });
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

        // TODO this.dbService.NotifyOfChange(ApplicationDataType.Budget);
        return CreateUserDefinedWidget(description, bucket.Code);
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

    private static string BuildMultiUseWidgetKey(IUserDefinedWidget widget)
    {
        var baseWidget = (Widget)widget;
        return baseWidget.Category + baseWidget.Name + widget.Id;
    }
}
