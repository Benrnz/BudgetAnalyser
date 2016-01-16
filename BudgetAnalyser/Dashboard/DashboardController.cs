using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Rees.Wpf;
using ApplicationStateLoadedMessage = BudgetAnalyser.ApplicationState.ApplicationStateLoadedMessage;
using ApplicationStateRequestedMessage = BudgetAnalyser.ApplicationState.ApplicationStateRequestedMessage;

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
        // TODO Support for image changes when widget updates

        public DashboardController(
            [NotNull] IUiContext uiContext,
            [NotNull] IDashboardService dashboardService,
            [NotNull] IApplicationDatabaseService applicationDatabaseService)
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

            RegisterForMessengerNotifications(uiContext.Messenger);
        }

        public Guid CorrelationId
        {
            get { return this.doNotUseCorrelationId; }
            private set
            {
                this.doNotUseCorrelationId = value;
                RaisePropertyChanged();
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
                RaisePropertyChanged();
            }
        }

        public string VersionString
        {
            get
            {
                AssemblyName assemblyName = GetType().Assembly.GetName();
                return assemblyName.Name + "Version: " + assemblyName.Version;
            }
        }

        [UsedImplicitly]
        public ICommand WidgetCommand => new RelayCommand<Widget>(OnWidgetCommandExecuted, WidgetCommandCanExecute);

        public ObservableCollection<WidgetGroup> WidgetGroups { get; private set; }

        private static bool WidgetCommandCanExecute(Widget widget)
        {
            return widget.Clickable;
        }

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
                WidgetGroups = this.dashboardService.LoadPersistedStateData(storedWidgetsState);
            }
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            WidgetsApplicationState widgetStates = this.dashboardService.PreparePersistentStateData();
            message.PersistThisModel(widgetStates);
        }

        private void OnBudgetBucketChosenForNewBucketMonitor(object sender, BudgetBucketChosenEventArgs args)
        {
            if (args.CorrelationId != CorrelationId)
            {
                return;
            }

            CorrelationId = Guid.NewGuid();
            BudgetBucket bucket = this.chooseBudgetBucketController.Selected;
            if (bucket == null)
            {
                // Cancelled by user.
                return;
            }

            Widget widget = this.dashboardService.CreateNewBucketMonitorWidget(bucket.Code);
            if (widget == null)
            {
                this.uiContext.UserPrompts.MessageBox.Show("New Budget Bucket Widget", "This Budget Bucket Monitor Widget for [{0}] already exists.", bucket.Code);
            }
        }

        private void OnBudgetReadyMessageReceived([NotNull] BudgetReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Budgets == null)
            {
                throw new InvalidOperationException("The budgets collection should never be null.");
            }

            this.dashboardService.NotifyOfDependencyChange(message.Budgets);
            this.dashboardService.NotifyOfDependencyChange<IBudgetCurrencyContext>(message.ActiveBudget);
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

        private void OnFilterAppliedMessageReceived([NotNull] FilterAppliedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Criteria == null)
            {
                throw new InvalidOperationException("The Criteria object should never be null.");
            }

            this.dashboardService.NotifyOfDependencyChange(message.Criteria);
        }

        private void OnLedgerBookReadyMessageReceived([NotNull] LedgerBookReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.LedgerBook == null)
            {
                this.dashboardService.NotifyOfDependencyChange<Engine.Ledger.LedgerBook>(null);
            }
            else
            {
                this.dashboardService.NotifyOfDependencyChange(message.LedgerBook);
            }
        }

        private void OnStatementModifiedMessagedReceived([NotNull] StatementHasBeenModifiedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.StatementModel == null)
            {
                return;
            }

            this.dashboardService.NotifyOfDependencyChange(message.StatementModel);
        }

        private void OnStatementReadyMessageReceived([NotNull] StatementReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.StatementModel == null)
            {
                this.dashboardService.NotifyOfDependencyChange<StatementModel>(null);
            }
            else
            {
                this.dashboardService.NotifyOfDependencyChange(message.StatementModel);
            }
        }

        private void OnWidgetCommandExecuted(Widget widget)
        {
            MessengerInstance.Send(new WidgetActivatedMessage(widget));
        }

        private void RegisterForMessengerNotifications(IMessenger messenger)
        {
            // Register for all dependent objects change messages.
            MessengerInstance = messenger;
            MessengerInstance.Register<StatementReadyMessage>(this, OnStatementReadyMessageReceived);
            MessengerInstance.Register<StatementHasBeenModifiedMessage>(this, OnStatementModifiedMessagedReceived);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoadedMessageReceived);
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            MessengerInstance.Register<FilterAppliedMessage>(this, OnFilterAppliedMessageReceived);
            MessengerInstance.Register<LedgerBookReadyMessage>(this, OnLedgerBookReadyMessageReceived);
        }
    }
}