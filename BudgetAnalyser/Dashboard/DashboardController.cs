using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.Dashboard
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class DashboardController : ControllerBase, IShowableController
    {
        private readonly Dictionary<Type, object> availableDependencies = new Dictionary<Type, object>();
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly ChooseBudgetBucketController chooseBudgetBucketController;
        private readonly IUserMessageBox messageBox;
        private readonly IWidgetRepository widgetRepository;
        private Guid doNotUseCorrelationId;
        private bool doNotUseShown;
        private TimeSpan elapsedTime;
        private int filterChangeHash;
        private Guid statementChangeHash;
        private Timer updateTimer;
        // TODO Support for image changes when widget updates

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Timer is needed for the lifetime of the controller, and controller is single instance")
        ]
        public DashboardController(
            [NotNull] UiContext uiContext,
            [NotNull] IWidgetRepository widgetRepository,
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] ChooseBudgetBucketController chooseBudgetBucketController)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (widgetRepository == null)
            {
                throw new ArgumentNullException("widgetRepository");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            if (chooseBudgetBucketController == null)
            {
                throw new ArgumentNullException("chooseBudgetBucketController");
            }

            this.widgetRepository = widgetRepository;
            this.widgetRepository.WidgetRemoved += OnBudgetBucketMonitorWidgetRemoved;
            this.bucketRepository = bucketRepository;
            this.chooseBudgetBucketController = chooseBudgetBucketController;
            this.chooseBudgetBucketController.Chosen += OnBudgetBucketMonitorWidgetAdded;
            GlobalFilterController = uiContext.GlobalFilterController;
            CorrelationId = Guid.NewGuid();
            this.messageBox = uiContext.UserPrompts.MessageBox;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<StatementReadyMessage>(this, OnStatementReadyMessageReceived);
            MessengerInstance.Register<StatementHasBeenModifiedMessage>(this, OnStatementModifiedMessagedReceived);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoadedMessageReceived);
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            MessengerInstance.Register<FilterAppliedMessage>(this, OnFilterAppliedMessageReceived);
            MessengerInstance.Register<LedgerBookReadyMessage>(this, OnLedgerBookReadyMessageReceived);


            this.updateTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = true,
            };
            this.updateTimer.Elapsed += OnUpdateTimerElapsed;

            InitialiseSupportedDependenciesArray();
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

        public ObservableCollection<Widget> Widgets { get; private set; }

        private static WidgetState CreateWidgetState(Widget widget)
        {
            var multiInstanceWidget = widget as IMultiInstanceWidget;
            if (multiInstanceWidget != null)
            {
                return new MultiInstanceWidgetState
                {
                    Id = multiInstanceWidget.Id,
                    Visible = multiInstanceWidget.Visibility,
                    WidgetType = multiInstanceWidget.WidgetType.FullName,
                };
            }

            return new WidgetState
            {
                Visible = widget.Visibility,
                WidgetType = widget.GetType().FullName,
            };
        }

        private static bool WidgetCommandCanExecute(Widget widget)
        {
            return widget.Clickable;
        }

        private void InitialiseSupportedDependenciesArray()
        {
            this.availableDependencies[typeof(StatementModel)] = null;
            this.availableDependencies[typeof(BudgetCollection)] = null;
            this.availableDependencies[typeof(BudgetCurrencyContext)] = null;
            this.availableDependencies[typeof(Engine.Ledger.LedgerBook)] = null;
            this.availableDependencies[typeof(IBudgetBucketRepository)] = this.bucketRepository;
            this.availableDependencies[typeof(GlobalFilterCriteria)] = null;
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

            List<Widget> widgets = this.widgetRepository.GetAll().ToList();

            foreach (WidgetState widgetState in storedState.WidgetStates)
            {
                WidgetState stateClone = widgetState;
                var multiInstanceState = widgetState as MultiInstanceWidgetState;
                if (multiInstanceState != null)
                {
                    // MultiInstance widgets need to be created at this point.  The App State data is required to create them.
                    IMultiInstanceWidget newIdWidget = this.widgetRepository.Create(multiInstanceState.WidgetType, multiInstanceState.Id);
                    newIdWidget.Visibility = multiInstanceState.Visible;
                }
                else
                {
                    // Ordinary widgets will already exist in the repository as they are single instance per class.
                    Widget typedWidget = widgets.FirstOrDefault(w => w.GetType().FullName == stateClone.WidgetType);
                    if (typedWidget != null)
                    {
                        typedWidget.Visibility = widgetState.Visible;
                    }
                }
            }

            Widgets = new ObservableCollection<Widget>(this.widgetRepository.GetAll());
            UpdateWidgets();
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            IEnumerable<WidgetState> widgetStates = Widgets.Select(CreateWidgetState);

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
            if (Widgets.OfType<BudgetBucketMonitorWidget>().Any(w => w.BucketCode == bucket.Code))
            {
                this.messageBox.Show("New Budget Bucket Widget", "This Budget Bucket Monitor Widget for [{0}] already exists.", bucket.Code);
                return;
            }

            IMultiInstanceWidget widget = this.widgetRepository.Create(typeof(BudgetBucketMonitorWidget).FullName, bucket.Code);
            var baseWidget = (Widget)widget;
            Widgets.Add(baseWidget);
            UpdateWidget(baseWidget);
        }

        private void OnBudgetBucketMonitorWidgetRemoved(object sender, WidgetRepositoryChangedEventArgs eventArgs)
        {
            Widgets.Remove(eventArgs.WidgetRemoved);
        }

        private void OnBudgetReadyMessageReceived([NotNull] BudgetReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            this.availableDependencies[typeof(BudgetCollection)] = message.Budgets;
            this.availableDependencies[typeof(BudgetCurrencyContext)] = message.ActiveBudget;
            UpdateWidgets(typeof(BudgetCollection), typeof(BudgetCurrencyContext));
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

        private void OnUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.elapsedTime = this.elapsedTime.Add(TimeSpan.FromMinutes(1));
            foreach (Widget widget in Widgets.Where(w => w.RecommendedTimeIntervalUpdate != null))
            {
                Debug.Assert(widget.RecommendedTimeIntervalUpdate != null, "widget.RecommendedTimeIntervalUpdate != null");
                if (this.elapsedTime >= widget.RecommendedTimeIntervalUpdate.Value)
                {
                    Widget widgetCopy = widget;
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => UpdateWidget(widgetCopy));
                }
            }
        }

        private void OnWidgetCommandExecuted(Widget widget)
        {
            MessengerInstance.Send(new WidgetActivatedMessage(widget));
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
            if (Widgets == null || !Widgets.Any())
            {
                return;
            }

            if (filterDependencyTypes != null && filterDependencyTypes.Length > 0)
            {
                // targeted update
                List<Widget> affectedWidgets = Widgets.Where(w => w.Dependencies.Any(filterDependencyTypes.Contains)).ToList();
                affectedWidgets.ForEach(UpdateWidget);
            }
            else
            {
                // update all
                Widgets.ToList().ForEach(UpdateWidget);
            }
        }
    }
}