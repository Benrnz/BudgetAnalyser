using System.Collections.ObjectModel;
using System.Windows.Input;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Filtering;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Dashboard
{
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
            [NotNull] IUiContext uiContext,
            [NotNull] IDashboardService dashboardService,
            [NotNull] IApplicationDatabaseService applicationDatabaseService)
            : base(uiContext.Messenger)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (dashboardService == null)
            {
                throw new ArgumentNullException(nameof(dashboardService));
            }

            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabaseService));
            }

            this.chooseBudgetBucketController = uiContext.ChooseBudgetBucketController;
            this.createNewFixedBudgetController = uiContext.CreateNewFixedBudgetController;
            this.createNewSurprisePaymentMonitorController = uiContext.CreateNewSurprisePaymentMonitorController;
            GlobalFilterController = uiContext.GlobalFilterController;

            this.uiContext = uiContext;
            this.dashboardService = dashboardService;

            this.chooseBudgetBucketController.Chosen += OnBudgetBucketChosenForNewBucketMonitor;
            this.createNewFixedBudgetController.Complete += OnCreateNewFixedProjectComplete;
            this.createNewSurprisePaymentMonitorController.Complete += OnCreateNewSurprisePaymentMonitorComplete;

            CorrelationId = Guid.NewGuid();

            RegisterForMessengerNotifications();
        }

        public Guid CorrelationId
        {
            get { return this.doNotUseCorrelationId; }
            private set
            {
                this.doNotUseCorrelationId = value;
                OnPropertyChanged();
            }
        }

        public GlobalFilterController GlobalFilterController { [UsedImplicitly] get; private set; }

        public bool Shown
        {
            get { return this.doNotUseShown; }
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

        public string VersionString
        {
            get
            {
                var assemblyName = GetType().Assembly.GetName();
                return assemblyName.Name + "Version: " + assemblyName.Version;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Used by data binding")]
        [UsedImplicitly]
        public ICommand WidgetActivatedCommand => WidgetCommands.WidgetClickedCommand;

        public ObservableCollection<WidgetGroup> WidgetGroups { get; private set; }

        private void OnApplicationStateLoadedMessageReceived([NotNull] ApplicationStateLoadedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var storedWidgetsState = message.ElementOfType<WidgetsApplicationState>();
            if (storedWidgetsState != null)
            {
                // Now that we have the previously persisted state data we can properly intialise the service.
                WidgetCommands.DeregisterForWidgetChanges(WidgetGroups);
                WidgetGroups = this.dashboardService.LoadPersistedStateData(storedWidgetsState);
                WidgetCommands.ListenForWidgetChanges(WidgetGroups);
            }
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            var widgetStates = this.dashboardService.PreparePersistentStateData();
            message.PersistThisModel(widgetStates);
        }

        private void OnBudgetBucketChosenForNewBucketMonitor(object sender, BudgetBucketChosenEventArgs args)
        {
            if (args.CorrelationId != CorrelationId)
            {
                return;
            }

            CorrelationId = Guid.NewGuid();
            var bucket = this.chooseBudgetBucketController.Selected;
            if (bucket == null)
            {
                // Cancelled by user.
                return;
            }

            var widget = this.dashboardService.CreateNewBucketMonitorWidget(bucket.Code);
            if (widget == null)
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
            try
            {
                this.dashboardService.CreateNewFixedBudgetMonitorWidget(
                    this.createNewFixedBudgetController.Code,
                    this.createNewFixedBudgetController.Description,
                    this.createNewFixedBudgetController.Amount);
            }
            catch (ArgumentException ex)
            {
                this.uiContext.UserPrompts.MessageBox.Show(ex.Message, "Unable to create new fixed budget project");
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

        private void RegisterForMessengerNotifications()
        {
            // Register for all dependent objects change messages.
            Messenger.Register<DashboardController, ApplicationStateLoadedMessage>(this, static (r, m) => r.OnApplicationStateLoadedMessageReceived(m));
            Messenger.Register<DashboardController, ApplicationStateRequestedMessage>(this, static (r, m) => r.OnApplicationStateRequested(m));
        }
    }
}