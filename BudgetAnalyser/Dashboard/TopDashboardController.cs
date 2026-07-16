using System.Collections.ObjectModel;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.Matching;
using BudgetAnalyser.Mobile;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Dashboard;

//[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Necessary in this case, this class is used to monitor all parts of the system.")]
[AutoRegisterWithIoC(SingleInstance = true)]
public sealed class TopDashboardController : ControllerBase, IShowableController
{
    private readonly IDashboardService dashboardService;
    private readonly DisusedRulesController disusedRulesController;
    private readonly ILogger logger;
    private readonly PersistenceOperations persistenceOperations;
    private readonly UploadMobileDataController uploadMobileDataController;
    private readonly IUserMessageBox userMessageBox;

    private Guid correlationId;

    public TopDashboardController(
        IMessenger messenger,
        ILogger logger,
        UserPrompts userPrompts,
        DisusedRulesController disusedRulesController,
        GlobalFilterController globalFilterController,
        UploadMobileDataController uploadMobileDataController,
        IDashboardService dashboardService,
        PersistenceOperations persistenceOperations) : base(messenger)
    {
        this.disusedRulesController = disusedRulesController ?? throw new ArgumentNullException(nameof(disusedRulesController));
        this.uploadMobileDataController = uploadMobileDataController ?? throw new ArgumentNullException(nameof(uploadMobileDataController));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.persistenceOperations = persistenceOperations ?? throw new ArgumentNullException(nameof(persistenceOperations));
        GlobalFilterController = globalFilterController ?? throw new ArgumentNullException(nameof(globalFilterController));

        this.dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        this.userMessageBox = userPrompts.MessageBox ?? throw new ArgumentNullException(nameof(userPrompts.MessageBox));
        this.dashboardService.NewDataSourceAvailable += OnNewDataSourceAvailable;

        this.correlationId = Guid.NewGuid();
        WidgetGroups = new ObservableCollection<WidgetGroup>();

        Messenger.Register<TopDashboardController, WidgetActivatedMessage>(this, OnWidgetActivatedMessageReceived);
        Messenger.Register<TopDashboardController, BudgetBucketChosenMessage>(this, OnBudgetBucketChosenForNewBucketMonitor);
        Messenger.Register<TopDashboardController, CreateNewFixedBudgetCompletedMessage>(this, OnCreateNewFixedProjectComplete);
        Messenger.Register<TopDashboardController, CreateNewSurprisePaymentCompletedMessage>(this, OnCreateNewSurprisePaymentMonitorComplete);
        Messenger.Register<TopDashboardController, ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
        Messenger.Register<TopDashboardController, ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
    }

    public GlobalFilterController GlobalFilterController
    {
        [UsedImplicitly]
        get;
        private set;
    }

    public string VersionString
    {
        get
        {
            var assemblyName = GetType().Assembly.GetName();
            return assemblyName.Name + "Version: " + assemblyName.Version;
        }
    }

    public ObservableCollection<WidgetGroup> WidgetGroups { get; private set; }

    public bool Shown
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    private void ObserveUnhandledFireAndForgetFailure(Task task, string context)
    {
        _ = task.ContinueWith(
            t =>
            {
                var baseException = t.Exception?.GetBaseException();
                if (baseException is not null)
                {
                    this.logger.LogError(baseException, _ => context);
                }
            },
            TaskContinuationOptions.OnlyOnFaulted);
    }

    private static void OnApplicationStateLoaded(TopDashboardController recipient, ApplicationStateLoadedMessage message)
    {
        recipient.ObserveUnhandledFireAndForgetFailure(
            recipient.OnApplicationStateLoadedAsync(message),
            "Unhandled exception processing ApplicationStateLoadedMessage in TopDashboardController.");
    }

    private async Task OnApplicationStateLoadedAsync(ApplicationStateLoadedMessage message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var storedMainAppState = message.ElementOfType<ApplicationEngineState>();
        if (storedMainAppState is not null)
        {
            await this.persistenceOperations.LoadDatabase(storedMainAppState.BudgetAnalyserDataStorageKey);
        }
    }

    private static void OnApplicationStateRequested(TopDashboardController recipient, ApplicationStateRequestedMessage message)
    {
        var dataFileState = recipient.persistenceOperations.PreparePersistentStateData();
        message.PersistThisModel(dataFileState);
    }

    private static void OnBudgetBucketChosenForNewBucketMonitor(TopDashboardController recipient, BudgetBucketChosenMessage message)
    {
        if (message.CorrelationId != recipient.correlationId || message.Canceled)
        {
            return;
        }

        recipient.correlationId = Guid.NewGuid();
        var bucket = message.SelectedBucket;
        if (bucket is null)
        {
            // Cancelled by user.
            return;
        }

        var widget = recipient.dashboardService.CreateNewBucketMonitorWidget(bucket.Code);
        if (widget is null)
        {
            recipient.userMessageBox.Show("New Budget Bucket Widget", "This Budget Bucket Monitor Widget for [{0}] already exists.", bucket.Code);
        }
    }

    private static void OnCreateNewFixedProjectComplete(TopDashboardController recipient, CreateNewFixedBudgetCompletedMessage message)
    {
        if (message.Canceled || message.CorrelationId != recipient.correlationId)
        {
            return;
        }

        recipient.correlationId = Guid.NewGuid();
        var widget = recipient.dashboardService.CreateNewFixedBudgetMonitorWidget(
            message.Code,
            message.Description,
            message.Amount);
        if (widget is null)
        {
            recipient.userMessageBox.Show($"A new fixed budget project bucket cannot be created, because the code {message.Code} already exists.");
        }
    }

    private void OnCreateNewSurprisePaymentMonitorComplete(object recipient, CreateNewSurprisePaymentCompletedMessage message)
    {
        if (message.Canceled || message.CorrelationId != this.correlationId)
        {
            return;
        }

        this.correlationId = Guid.NewGuid();
        try
        {
            if (string.IsNullOrWhiteSpace(message.BucketCode))
            {
                return;
            }

            this.dashboardService.CreateNewSurprisePaymentMonitorWidget(
                message.BucketCode,
                message.PaymentStartDate,
                message.Frequency);
        }
        catch (ArgumentException ex)
        {
            this.userMessageBox.Show(ex.Message, "Unable to create new surprise payment monitor widget.");
        }
    }

    private void OnNewDataSourceAvailable(object? sender, EventArgs? eventArgs)
    {
        WidgetCommands.DeregisterForWidgetChanges(WidgetGroups);
        WidgetGroups = this.dashboardService.WidgetsToDisplay();
        OnPropertyChanged(nameof(WidgetGroups));
        WidgetCommands.ListenForWidgetChanges(WidgetGroups);
    }

    private void OnWidgetActivatedMessageReceived(TopDashboardController recipient, WidgetActivatedMessage message)
    {
        if (message.Handled)
        {
            return;
        }

        if (message.Widget is SaveWidget)
        {
            ObserveUnhandledFireAndForgetFailure(this.persistenceOperations.OnSaveDatabaseCommandExecute(), "Unhandled exception processing Save in TopDashboardController.");
            return;
        }

        if (message.Widget is DaysSinceLastImport)
        {
            Messenger.Send(new MainMenuTabRequestMessage(MainMenuTab.Transactions));
            return;
        }

        if (message.Widget is CurrentFileWidget)
        {
            ObserveUnhandledFireAndForgetFailure(ProcessCurrentFileWidgetActivated(message), "Unhandled exception processing CurrentFileWidget in TopDashboardController.");
            return;
        }

        if (message.Widget is LoadDemoWidget)
        {
            ObserveUnhandledFireAndForgetFailure(ProcessLoadDemoWidgetActivated(message), "Unhandled exception processing LoadDemoWidget in TopDashboardController.");
            return;
        }

        if (message.Widget is NewFileWidget)
        {
            ObserveUnhandledFireAndForgetFailure(ProcessCreateNewFileWidgetActivated(message), "Unhandled exception processing NewFileWidget in TopDashboardController.");
            return;
        }

        if (message.Widget is DisusedMatchingRuleWidget)
        {
            this.disusedRulesController.ShowDialog();
            return;
        }

        if (message.Widget is UpdateMobileDataWidget mobileWidget)
        {
            this.uploadMobileDataController.ShowDialog(mobileWidget);
        }
    }

    private async Task ProcessCreateNewFileWidgetActivated(WidgetActivatedMessage message)
    {
        if (message.Widget is not NewFileWidget)
        {
            return;
        }

        message.Handled = true;

        await this.persistenceOperations.OnCreateNewDatabaseCommandExecute();
    }

    private async Task ProcessCurrentFileWidgetActivated(WidgetActivatedMessage message)
    {
        // Open new Database file
        if (message.Widget is not CurrentFileWidget)
        {
            return;
        }

        message.Handled = true;

        await this.persistenceOperations.OnLoadDatabaseCommandExecute();
    }

    private async Task ProcessLoadDemoWidgetActivated(WidgetActivatedMessage message)
    {
        if (message.Widget is not LoadDemoWidget)
        {
            return;
        }

        message.Handled = true;

        await this.persistenceOperations.OnLoadDemoDatabaseCommandExecute();
    }
}
