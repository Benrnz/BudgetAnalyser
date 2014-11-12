using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Necessary in this case, this class is used to monitor all parts of the system.")]
    [AutoRegisterWithIoC(SingleInstance = true)]
    public sealed class DashboardController : ControllerBase, IShowableController
    {
        private readonly Dictionary<Type, object> availableDependencies = new Dictionary<Type, object>();
        private readonly ChooseBudgetBucketController chooseBudgetBucketController;
        private readonly IDashboardService dashboardService;
        private readonly IUserMessageBox messageBox;
        private readonly IWidgetRepository widgetRepository;
        private readonly WidgetService widgetService;
        private Guid doNotUseCorrelationId;
        private bool doNotUseShown;
        private TimeSpan elapsedTime;
        private int filterChangeHash;
        private Guid statementChangeHash;
        // TODO Support for image changes when widget updates

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
            Justification = "This class monitors and displays health and metrics for the whole app, so is linked to many classes.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Timer is needed for the lifetime of the controller, and controller is single instance")
        ]
        public DashboardController(
            [NotNull] UiContext uiContext,
            [NotNull] IWidgetRepository widgetRepository,
            [NotNull] ChooseBudgetBucketController chooseBudgetBucketController,
            [NotNull] WidgetService widgetService,
            [NotNull] IDashboardService dashboardService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (widgetRepository == null)
            {
                throw new ArgumentNullException("widgetRepository");
            }

            if (chooseBudgetBucketController == null)
            {
                throw new ArgumentNullException("chooseBudgetBucketController");
            }

            if (widgetService == null)
            {
                throw new ArgumentNullException("widgetService");
            }

            if (dashboardService == null)
            {
                throw new ArgumentNullException("dashboardService");
            }

            this.widgetRepository = widgetRepository;
            this.chooseBudgetBucketController = chooseBudgetBucketController;
            this.messageBox = uiContext.UserPrompts.MessageBox;
            this.dashboardService = dashboardService;
            this.widgetService = widgetService;

            this.widgetRepository.WidgetRemoved += OnBudgetBucketMonitorWidgetRemoved;
            this.chooseBudgetBucketController.Chosen += OnBudgetBucketMonitorWidgetAdded;

            GlobalFilterController = uiContext.GlobalFilterController;
            CorrelationId = Guid.NewGuid();

            RegisterForMessengerNotifications(uiContext);

            this.dashboardService.WidgetRefreshRequested += OnWidgetUpdateRequested;
            this.dashboardService.InitialiseSupportedDependenciesArray();
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

            WidgetGroups = this.dashboardService.ViewWidgetGroups(storedState.WidgetStates);
            UpdateWidgets();
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            IEnumerable<WidgetPersistentState> widgetStates = this.dashboardService.PreparePersistentData(WidgetGroups);

            message.PersistThisModel(
                new DashboardApplicationStateV1
                {
                    Model = new DashboardApplicationStateModel { WidgetStates = widgetStates.ToList() }
                });
        }

        private void OnBudgetBucketMonitorWidgetAdded(object sender, BudgetBucketChosenEventArgs args)
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

            var baseWidget = this.dashboardService.CreateNewBucketMonitorWidget(WidgetGroups, bucket.Code);
            if (baseWidget == null)
            {
                this.messageBox.Show("New Budget Bucket Widget", "This Budget Bucket Monitor Widget for [{0}] already exists.", bucket.Code);
                return;
            }

            UpdateWidget(baseWidget);
        }

        private void OnBudgetBucketMonitorWidgetRemoved(object sender, WidgetRepositoryChangedEventArgs eventArgs)
        {
            this.dashboardService.RemoveBucketMonitorWidget(WidgetGroups, eventArgs.WidgetRemoved);
        }

        private void OnBudgetReadyMessageReceived([NotNull] BudgetReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            this.availableDependencies[typeof(BudgetCollection)] = message.Budgets;
            this.availableDependencies[typeof(IBudgetCurrencyContext)] = message.ActiveBudget;
            UpdateWidgets(typeof(BudgetCollection), typeof(IBudgetCurrencyContext));
        }

        private void OnFilterAppliedMessageReceived([NotNull] FilterAppliedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Type key = typeof(GlobalFilterCriteria);
            this.availableDependencies[key] = message.Criteria;

            int newHash = message.Criteria.GetHashCode();
            if (newHash != this.filterChangeHash)
            {
                this.filterChangeHash = newHash;
                UpdateWidgets(key);
            }
        }

        private void OnLedgerBookReadyMessageReceived([NotNull] LedgerBookReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            this.availableDependencies[typeof(Engine.Ledger.LedgerBook)] = message.LedgerBook;
            UpdateWidgets(typeof(Engine.Ledger.LedgerBook));
        }

        private void OnStatementModifiedMessagedReceived([NotNull] StatementHasBeenModifiedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (message.StatementModel == null)
            {
                return;
            }

            if (this.statementChangeHash != message.StatementModel.ChangeHash)
            {
                this.statementChangeHash = message.StatementModel.ChangeHash;
                Type key = typeof(StatementModel);
                this.availableDependencies[key] = message.StatementModel;
                UpdateWidgets(key);
            }
        }

        private void OnStatementReadyMessageReceived([NotNull] StatementReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Type key = typeof(StatementModel);
            this.availableDependencies[key] = message.StatementModel;
            UpdateWidgets(key);
        }

        private void OnWidgetCommandExecuted(Widget widget)
        {
            MessengerInstance.Send(new WidgetActivatedMessage(widget));
        }

        private void OnWidgetUpdateRequested(object sender, EventArgs e)
        {
            this.elapsedTime = this.elapsedTime.Add(TimeSpan.FromMinutes(1));
            foreach (Widget widget in WidgetGroups.SelectMany(group => group.Widgets).Where(w => w.RecommendedTimeIntervalUpdate != null))
            {
                Debug.Assert(widget.RecommendedTimeIntervalUpdate != null, "widget.RecommendedTimeIntervalUpdate != null");
                if (this.elapsedTime >= widget.RecommendedTimeIntervalUpdate.Value)
                {
                    Widget widgetCopy = widget;
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => UpdateWidget(widgetCopy));
                }
            }
        }

        private void RegisterForMessengerNotifications(UiContext uiContext)
        {
            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<StatementReadyMessage>(this, OnStatementReadyMessageReceived);
            MessengerInstance.Register<StatementHasBeenModifiedMessage>(this, OnStatementModifiedMessagedReceived);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoadedMessageReceived);
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            MessengerInstance.Register<FilterAppliedMessage>(this, OnFilterAppliedMessageReceived);
            MessengerInstance.Register<LedgerBookReadyMessage>(this, OnLedgerBookReadyMessageReceived);
        }

        private void UpdateWidget(Widget widget)
        {
            if (widget.Dependencies == null || !widget.Dependencies.Any())
            {
                widget.Update();
                return;
            }

            var parameters = new object[widget.Dependencies.Count()];
            int index = 0;
            foreach (Type dependencyType in widget.Dependencies)
            {
                if (!this.availableDependencies.ContainsKey(dependencyType))
                {
                    // If you get an exception here first check the InitialiseSupportedDependenciesArray method.
                    throw new NotSupportedException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "The requested dependency {0} for the widget {1} is not supported.",
                            dependencyType.Name,
                            widget.Name));
                }

                parameters[index++] = this.availableDependencies[dependencyType];
            }

            widget.Update(parameters);
        }

        private void UpdateWidgets(params Type[] filterDependencyTypes)
        {
            if (WidgetGroups == null)
            {
                return;
            }

            if (!WidgetGroups.Any())
            {
                return;
            }

            if (filterDependencyTypes != null && filterDependencyTypes.Length > 0)
            {
                // targeted update
                List<Widget> affectedWidgets = WidgetGroups.SelectMany(group => group.Widgets)
                    .Where(w => w.Dependencies.Any(filterDependencyTypes.Contains))
                    .ToList();
                affectedWidgets.ForEach(UpdateWidget);
            }
            else
            {
                // update all
                WidgetGroups.SelectMany(group => group.Widgets).ToList().ForEach(UpdateWidget);
            }
        }
    }
}