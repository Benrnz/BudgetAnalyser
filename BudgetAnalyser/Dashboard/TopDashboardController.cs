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
    private readonly CreateNewSurprisePaymentMonitorController createNewSurprisePaymentMonitorController;
    private readonly IDashboardService dashboardService;
    private readonly DisusedRulesController disusedRulesController;
    private readonly PersistenceOperations persistenceOperations;
    private readonly UploadMobileDataController uploadMobileDataController;
    private readonly IUserMessageBox userMessageBox;

    private Guid correlationId;

    public TopDashboardController(
        IMessenger messenger,
        UserPrompts userPrompts,
        CreateNewSurprisePaymentMonitorController createNewSurprisePaymentMonitorController,
        DisusedRulesController disusedRulesController,
        GlobalFilterController globalFilterController,
        UploadMobileDataController uploadMobileDataController,
        IDashboardService dashboardService,
        PersistenceOperations persistenceOperations) : base(messenger)
    {
        this.createNewSurprisePaymentMonitorController = createNewSurprisePaymentMonitorController ?? throw new ArgumentNullException(nameof(createNewSurprisePaymentMonitorController));
        this.disusedRulesController = disusedRulesController ?? throw new ArgumentNullException(nameof(disusedRulesController));
        this.uploadMobileDataController = uploadMobileDataController ?? throw new ArgumentNullException(nameof(uploadMobileDataController));
        this.persistenceOperations = persistenceOperations ?? throw new ArgumentNullException(nameof(persistenceOperations));
        GlobalFilterController = globalFilterController ?? throw new ArgumentNullException(nameof(globalFilterController));

        this.dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        this.userMessageBox = userPrompts.MessageBox ?? throw new ArgumentNullException(nameof(userPrompts.MessageBox));
        this.dashboardService.NewDataSourceAvailable += OnNewDataSourceAvailable;

        this.createNewSurprisePaymentMonitorController.Complete += OnCreateNewSurprisePaymentMonitorComplete;

        this.correlationId = Guid.NewGuid();
        WidgetGroups = new ObservableCollection<WidgetGroup>();

        Messenger.Register<TopDashboardController, WidgetActivatedMessage>(this, static (r, m) => r.OnWidgetActivatedMessageReceived(m).Wait());
        Messenger.Register<TopDashboardController, BudgetBucketChosenMessage>(this, static (r, m) => r.OnBudgetBucketChosenForNewBucketMonitor(m));
        Messenger.Register<TopDashboardController, CreateNewFixedBudgetCompletedMessage>(this, static (r, m) => r.OnCreateNewFixedProjectComplete(m));
        Messenger.Register<TopDashboardController, ApplicationStateLoadedMessage>(this, static (r, m) => r.OnApplicationStateLoaded(m).Wait());
        Messenger.Register<TopDashboardController, ApplicationStateRequestedMessage>(this, static (r, m) => r.OnApplicationStateRequested(m));

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

    private async Task OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
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

    private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
    {
        var dataFileState = this.persistenceOperations.PreparePersistentStateData();
        message.PersistThisModel(dataFileState);
    }

    private void OnBudgetBucketChosenForNewBucketMonitor(BudgetBucketChosenMessage message)
    {
        if (message.CorrelationId != this.correlationId || message.Canceled)
        {
            return;
        }

        this.correlationId = Guid.NewGuid();
        var bucket = message.SelectedBucket;
        if (bucket is null)
        {
            // Cancelled by user.
            return;
        }

        var widget = this.dashboardService.CreateNewBucketMonitorWidget(bucket.Code);
        if (widget is null)
        {
            this.userMessageBox.Show("New Budget Bucket Widget", "This Budget Bucket Monitor Widget for [{0}] already exists.", bucket.Code);
        }
    }

    private void OnCreateNewFixedProjectComplete(CreateNewFixedBudgetCompletedMessage message)
    {
        if (message.Canceled || message.CorrelationId != this.correlationId)
        {
            return;
        }

        this.correlationId = Guid.NewGuid();
        var widget = this.dashboardService.CreateNewFixedBudgetMonitorWidget(
            message.Code,
            message.Description,
            message.Amount);
        if (widget is null)
        {
            this.userMessageBox.Show($"A new fixed budget project bucket cannot be created, because the code {message.Code} already exists.");
        }
    }

    private void OnCreateNewSurprisePaymentMonitorComplete(object? sender, DialogResponseEventArgs dialogResponseEventArgs)
    {
        if (dialogResponseEventArgs.Canceled || dialogResponseEventArgs.CorrelationId != this.correlationId)
        {
            return;
        }

        this.correlationId = Guid.NewGuid();
        try
        {
            if (this.createNewSurprisePaymentMonitorController.Selected is null)
            {
                return;
            }

            this.dashboardService.CreateNewSurprisePaymentMonitorWidget(
                this.createNewSurprisePaymentMonitorController.Selected.Code,
                this.createNewSurprisePaymentMonitorController.PaymentStartDate,
                this.createNewSurprisePaymentMonitorController.Frequency);
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

    private async Task OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
    {
        if (message.Handled)
        {
            return;
        }

        if (message.Widget is SaveWidget)
        {
            await this.persistenceOperations.OnSaveDatabaseCommandExecute();
            return;
        }

        if (message.Widget is DaysSinceLastImport)
        {
            Messenger.Send(new MainMenuTabRequestMessage(MainMenuTab.Transactions));
            return;
        }

        if (message.Widget is CurrentFileWidget)
        {
            await ProcessCurrentFileWidgetActivated(message);
            return;
        }

        if (message.Widget is LoadDemoWidget)
        {
            await ProcessLoadDemoWidgetActivated(message);
            return;
        }

        if (message.Widget is NewFileWidget)
        {
            await ProcessCreateNewFileWidgetActivated(message);
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
