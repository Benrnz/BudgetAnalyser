using System.Collections.ObjectModel;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Filtering;
using Rees.Wpf;

namespace BudgetAnalyser.Dashboard;

//[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Necessary in this case, this class is used to monitor all parts of the system.")]
[AutoRegisterWithIoC(SingleInstance = true)]
public sealed class DashboardController : ControllerBase, IShowableController
{
    private readonly ChooseBudgetBucketController chooseBudgetBucketController;
    private readonly CreateNewFixedBudgetController createNewFixedBudgetController;
    private readonly CreateNewSurprisePaymentMonitorController createNewSurprisePaymentMonitorController;
    private readonly IDashboardService dashboardService;
    private readonly IUiContext uiContext;
    private Guid doNotUseCorrelationId;
    private bool doNotUseShown;

    public DashboardController(
        [NotNull]
        IUiContext uiContext,
        [NotNull]
        IDashboardService dashboardService)
        : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.chooseBudgetBucketController = uiContext.ChooseBudgetBucketController;
        this.createNewFixedBudgetController = uiContext.CreateNewFixedBudgetController;
        this.createNewSurprisePaymentMonitorController = uiContext.CreateNewSurprisePaymentMonitorController;
        GlobalFilterController = uiContext.GlobalFilterController;

        this.uiContext = uiContext;
        this.dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));

        this.chooseBudgetBucketController.Chosen += OnBudgetBucketChosenForNewBucketMonitor;
        this.createNewFixedBudgetController.Complete += OnCreateNewFixedProjectComplete;
        this.createNewSurprisePaymentMonitorController.Complete += OnCreateNewSurprisePaymentMonitorComplete;

        CorrelationId = Guid.NewGuid();
    }

    public Guid CorrelationId
    {
        get => this.doNotUseCorrelationId;
        private set
        {
            this.doNotUseCorrelationId = value;
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
        get => this.doNotUseShown;
        set
        {
            if (value == this.doNotUseShown)
            {
                return;
            }

            this.doNotUseShown = value;
            OnPropertyChanged();
        }
    }

    // TODO How to pass the data into this controller?
    // WidgetCommands.DeregisterForWidgetChanges(WidgetGroups);
    // WidgetGroups = this.dashboardService.WidgetsToDisplay(storedWidgetsState);
    // WidgetCommands.ListenForWidgetChanges(WidgetGroups);


    private void OnBudgetBucketChosenForNewBucketMonitor(object sender, BudgetBucketChosenEventArgs args)
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
            this.uiContext.UserPrompts.MessageBox.Show("New Budget Bucket Widget", "This Budget Bucket Monitor Widget for [{0}] already exists.", bucket.Code);
        }
    }

    private void OnCreateNewFixedProjectComplete(object sender, DialogResponseEventArgs dialogResponseEventArgs)
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
            this.uiContext.UserPrompts.MessageBox.Show($"A new fixed budget project bucket cannot be created, because the code {this.createNewFixedBudgetController.Code} already exists.");
        }
    }

    private void OnCreateNewSurprisePaymentMonitorComplete(object sender, DialogResponseEventArgs dialogResponseEventArgs)
    {
        if (dialogResponseEventArgs.Canceled || dialogResponseEventArgs.CorrelationId != CorrelationId)
        {
            return;
        }

        CorrelationId = Guid.NewGuid();
        try
        {
            this.dashboardService.CreateNewSurprisePaymentMonitorWidget(
                this.createNewSurprisePaymentMonitorController.Selected.Code,
                this.createNewSurprisePaymentMonitorController.PaymentStartDate,
                this.createNewSurprisePaymentMonitorController.Frequency);
        }
        catch (ArgumentException ex)
        {
            this.uiContext.UserPrompts.MessageBox.Show(ex.Message, "Unable to create new surprise payment monitor widget.");
        }
    }
}
