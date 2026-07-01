using System.Collections.ObjectModel;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.Matching;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Dashboard;

//[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Necessary in this case, this class is used to monitor all parts of the system.")]
[AutoRegisterWithIoC(SingleInstance = true)]
public sealed class TopDashboardController : ControllerBase, IShowableController
{
    private readonly ChooseBudgetBucketController chooseBudgetBucketController;
    private readonly CreateNewFixedBudgetController createNewFixedBudgetController;
    private readonly CreateNewSurprisePaymentMonitorController createNewSurprisePaymentMonitorController;
    private readonly IDashboardService dashboardService;
    private readonly DisusedRulesController disusedRulesController;
    private readonly IUserMessageBox userMessageBox;

    public TopDashboardController(IMessenger messenger, UserPrompts userPrompts, IUiContext uiContext, IDashboardService dashboardService) : base(messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.chooseBudgetBucketController = uiContext.Controller<ChooseBudgetBucketController>();
        this.createNewFixedBudgetController = uiContext.Controller<CreateNewFixedBudgetController>();
        this.createNewSurprisePaymentMonitorController = uiContext.Controller<CreateNewSurprisePaymentMonitorController>();
        this.disusedRulesController = uiContext.Controller<DisusedRulesController>();
        GlobalFilterController = uiContext.Controller<GlobalFilterController>();

        this.dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        this.userMessageBox = userPrompts.MessageBox ?? throw new ArgumentNullException(nameof(userPrompts.MessageBox));
        this.dashboardService.NewDataSourceAvailable += OnNewDataSourceAvailable;

        this.chooseBudgetBucketController.Chosen += OnBudgetBucketChosenForNewBucketMonitor;
        this.createNewFixedBudgetController.Complete += OnCreateNewFixedProjectComplete;
        this.createNewSurprisePaymentMonitorController.Complete += OnCreateNewSurprisePaymentMonitorComplete;

        CorrelationId = Guid.NewGuid();
        WidgetGroups = new ObservableCollection<WidgetGroup>();

        Messenger.Register<TopDashboardController, WidgetActivatedMessage>(this, static (r, m) => r.OnWidgetActivatedMessageReceived(m));
    }

    public Guid CorrelationId
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
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

    private void OnBudgetBucketChosenForNewBucketMonitor(object? sender, BudgetBucketChosenEventArgs args)
    {
        if (args.CorrelationId != CorrelationId)
        {
            return;
        }

        CorrelationId = Guid.NewGuid();
        var bucket = this.chooseBudgetBucketController.Selected;
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

    private void OnCreateNewFixedProjectComplete(object? sender, DialogResponseEventArgs dialogResponseEventArgs)
    {
        if (dialogResponseEventArgs.Canceled || dialogResponseEventArgs.CorrelationId != CorrelationId)
        {
            return;
        }

        CorrelationId = Guid.NewGuid();
        var widget = this.dashboardService.CreateNewFixedBudgetMonitorWidget(
            this.createNewFixedBudgetController.Code,
            this.createNewFixedBudgetController.Description,
            this.createNewFixedBudgetController.Amount);
        if (widget is null)
        {
            this.userMessageBox.Show($"A new fixed budget project bucket cannot be created, because the code {this.createNewFixedBudgetController.Code} already exists.");
        }
    }

    private void OnCreateNewSurprisePaymentMonitorComplete(object? sender, DialogResponseEventArgs dialogResponseEventArgs)
    {
        if (dialogResponseEventArgs.Canceled || dialogResponseEventArgs.CorrelationId != CorrelationId)
        {
            return;
        }

        CorrelationId = Guid.NewGuid();
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

    private void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
    {
        if (message.Handled || !(message.Widget is DisusedMatchingRuleWidget))
        {
            return;
        }

        this.disusedRulesController.ShowDialog();
    }
}
