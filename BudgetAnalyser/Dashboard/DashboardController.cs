using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Threading;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.Dashboard
{
    //[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Necessary in this case, this class is used to monitor all parts of the system.")]
    [AutoRegisterWithIoC(SingleInstance = true)]
    public sealed class DashboardController : ControllerBase, IShowableController
    {
        private readonly ChooseBudgetBucketController chooseBudgetBucketController;
        private readonly IDashboardService dashboardService;
        private readonly IUserMessageBox messageBox;
        private Guid doNotUseCorrelationId;
        private bool doNotUseShown;
        // TODO Support for image changes when widget updates

        public DashboardController(
            [NotNull] UiContext uiContext,
            [NotNull] ChooseBudgetBucketController chooseBudgetBucketController,
            [NotNull] IDashboardService dashboardService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (chooseBudgetBucketController == null)
            {
                throw new ArgumentNullException("chooseBudgetBucketController");
            }

            if (dashboardService == null)
            {
                throw new ArgumentNullException("dashboardService");
            }

            this.chooseBudgetBucketController = chooseBudgetBucketController;
            this.messageBox = uiContext.UserPrompts.MessageBox;
            this.dashboardService = dashboardService;

            this.chooseBudgetBucketController.Chosen += OnBudgetBucketChosenForNewBucketMonitor;

            GlobalFilterController = uiContext.GlobalFilterController;
            CorrelationId = Guid.NewGuid();

            RegisterForMessengerNotifications(uiContext);
        }

        public Guid CorrelationId
        {
            get { return this.doNotUseCorrelationId; }
            private set
            {
                this.doNotUseCorrelationId = value;
                RaisePropertyChanged(() => CorrelationId);
            }
        }

        public GlobalFilterController GlobalFilterController { get; private set; }

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
                RaisePropertyChanged(() => Shown);
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

        public ICommand WidgetCommand
        {
            get { return new RelayCommand<Widget>(OnWidgetCommandExecuted, WidgetCommandCanExecute); }
        }

        public ObservableCollection<WidgetGroup> WidgetGroups { get; private set; }

        private static bool WidgetCommandCanExecute(Widget widget)
        {
            return widget.Clickable;
        }

        private void OnApplicationStateLoadedMessageReceived([NotNull] ApplicationStateLoadedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (!message.RehydratedModels.ContainsKey(typeof(DashboardApplicationStateV1)))
            {
                return;
            }

            var storedState = message.RehydratedModels[typeof(DashboardApplicationStateV1)].AdaptModel<DashboardApplicationStateModel>();
            if (storedState == null)
            {
                return;
            }

            // Now that we have the previously persisted state data we can properly intialise the service.
            WidgetGroups = this.dashboardService.InitialiseWidgetGroups(storedState.WidgetStates);
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            IEnumerable<WidgetPersistentState> widgetStates = this.dashboardService.PreparePersistentStateData();

            message.PersistThisModel(
                new DashboardApplicationStateV1
                {
                    Model = new DashboardApplicationStateModel { WidgetStates = widgetStates.ToList() }
                });
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
                this.messageBox.Show("New Budget Bucket Widget", "This Budget Bucket Monitor Widget for [{0}] already exists.", bucket.Code);
            }
        }

        private void OnBudgetReadyMessageReceived([NotNull] BudgetReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (message.Budgets == null)
            {
                throw new InvalidOperationException("The budgets collection should never be null.");
            }

            this.dashboardService.NotifyOfDependencyChange(message.Budgets);
            this.dashboardService.NotifyOfDependencyChange<IBudgetCurrencyContext>(message.ActiveBudget);
        }

        private void OnFilterAppliedMessageReceived([NotNull] FilterAppliedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
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
                throw new ArgumentNullException("message");
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
                throw new ArgumentNullException("message");
            }

            // TODO Can this logic be moved to the Dashboard Service.
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
                throw new ArgumentNullException("message");
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

        private void RegisterForMessengerNotifications(UiContext uiContext)
        {
            // Register for all dependent objects change messages.
            MessengerInstance = uiContext.Messenger;
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