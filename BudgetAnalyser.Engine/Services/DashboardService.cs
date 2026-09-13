using System.Collections.ObjectModel;
using System.Text;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Engine.Services;

[AutoRegisterWithIoC(SingleInstance = true)]
// ReSharper disable once UnusedType.Global // Instantiated by IoC
internal class DashboardService : IDashboardService, ISupportsModelPersistence
{
    private readonly IDirtyDataService dirtyDataService;
    private readonly ILogger logger;
    private readonly IWidgetRepository widgetRepo;
    private readonly IWidgetService widgetService;
    private ObservableCollection<WidgetGroup> widgetGroups = new();

    public DashboardService(
        IWidgetService widgetService,
        ILogger logger,
        IWidgetRepository widgetRepo,
        IDirtyDataService dirtyDataService)
    {
        this.widgetService = widgetService ?? throw new ArgumentNullException(nameof(widgetService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.widgetRepo = widgetRepo ?? throw new ArgumentNullException(nameof(widgetRepo));
        this.dirtyDataService = dirtyDataService ?? throw new ArgumentNullException(nameof(dirtyDataService));
    }

    public event EventHandler? NewDataSourceAvailable;

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

        var widget = this.widgetService.CreateUserDefinedWidget(typeof(BudgetBucketMonitorWidget).FullName!, bucketCode);
        if (widget is null)
        {
            return null;
        }

        return UpdateWidgetCollectionWithNewAddition((Widget)widget);
    }

    /// <inheritdoc />
    public Widget? CreateNewFixedBudgetMonitorWidget(string bucketCode, string description, decimal fixedBudgetAmount)
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
        if (widget is null)
        {
            return null;
        }

        return UpdateWidgetCollectionWithNewAddition((Widget)widget);
    }

    /// <inheritdoc />
    public Widget? CreateNewSurprisePaymentMonitorWidget(string bucketCode, DateOnly paymentDate, WeeklyOrFortnightly frequency)
    {
        if (string.IsNullOrWhiteSpace(bucketCode))
        {
            throw new ArgumentNullException(nameof(bucketCode));
        }

        if (paymentDate == DateOnly.MinValue)
        {
            throw new ArgumentException("Payment date is not set.", nameof(paymentDate));
        }

        var widget = this.widgetService.CreateNewSurprisePaymentWidget(bucketCode, paymentDate, frequency);
        if (widget is null)
        {
            return null;
        }

        return UpdateWidgetCollectionWithNewAddition((Widget)widget);
    }

    /// <inheritdoc />
    public ObservableCollection<WidgetGroup> WidgetsToDisplay()
    {
        return this.widgetGroups;
    }

    /// <inheritdoc />
    public void RemoveUserDefinedWidget(IUserDefinedWidget widgetToRemove)
    {
        this.widgetService.RemoveUserDefinedWidget(widgetToRemove);

        var baseWidget = (Widget)widgetToRemove;
        var widgetGroup = this.widgetGroups.FirstOrDefault(group => group.Heading == baseWidget.Category);

        widgetGroup?.Widgets.Remove(baseWidget);
        this.dirtyDataService.NotifyOfChange(ApplicationDataType.Widgets);
        if (widgetToRemove is FixedBudgetMonitorWidget)
        {
            this.dirtyDataService.NotifyOfChange(ApplicationDataType.Budget);
        }
    }

    /// <inheritdoc />
    public void ShowAllWidgets()
    {
        this.widgetGroups
            .ToList()
            .ForEach(g => g.Widgets.ToList().ForEach(w => w.Visibility = true));
        this.dirtyDataService.NotifyOfChange(ApplicationDataType.Widgets);
    }

    /// <inheritdoc />
    public ApplicationDataType DataType => ApplicationDataType.Widgets;

    /// <inheritdoc />
    public int LoadSequence => 99;

    /// <inheritdoc />
    public void Close()
    {
        this.widgetService.CancelScheduledUpdates();
    }

    /// <inheritdoc />
    public async Task CreateNewAsync(ApplicationDatabase applicationDatabase)
    {
        await this.widgetRepo.CreateNewAndSaveAsync(applicationDatabase.WidgetsCollectionStorageKey);
        var handler = NewDataSourceAvailable;
        if (handler is not null)
        {
            handler(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public async Task LoadAsync(ApplicationDatabase applicationDatabase)
    {
        var widgets = await this.widgetRepo.LoadAsync(applicationDatabase.WidgetsCollectionStorageKey, applicationDatabase.IsEncrypted);
        this.widgetService.Initialise(widgets);
        this.widgetGroups = this.widgetService.ArrangeWidgetsForDisplay();

        var handler = NewDataSourceAvailable;
        if (handler is not null)
        {
            handler(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(ApplicationDatabase applicationDatabase)
    {
        await this.widgetRepo.SaveAsync(this.widgetGroups.SelectMany(g => g.Widgets), applicationDatabase.WidgetsCollectionStorageKey, applicationDatabase.IsEncrypted);
    }

    /// <inheritdoc />
    public void SavePreview()
    {
    }

    public bool ValidateModel(StringBuilder messages)
    {
        // Nothing to validate - all new additions are validated when created.
        return true;
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
        this.dirtyDataService.NotifyOfChange(ApplicationDataType.Widgets);
        this.widgetService.UpdateWidgetData(baseWidget);
        return baseWidget;
    }
}
