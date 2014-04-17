using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using BudgetAnalyser.Engine.Widget;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.Dashboard
{
    public class DashboardController : ControllerBase, IShowableController
    {
        private readonly Dictionary<Type, object> availableDependencies = new Dictionary<Type, object>();
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly IWidgetRepository widgetRepository;
        private bool doNotUseShown;
        private TimeSpan elapsedTime;
        private Guid statementChangeHash;
        private int filterChangeHash;
        private Timer updateTimer;
        // TODO Support for image changes when widget updates

        public DashboardController(UiContext uiContext, [NotNull] IWidgetRepository widgetRepository, [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (widgetRepository == null)
            {
                throw new ArgumentNullException("widgetRepository");
            }
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.widgetRepository = widgetRepository;
            this.bucketRepository = bucketRepository;
            GlobalFilterController = uiContext.GlobalFilterController;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<StatementReadyMessage>(this, OnStatementReadyMessageReceived);
            MessengerInstance.Register<StatementHasBeenModifiedMessage>(this, OnStatementModifiedMessagedReceived);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoadedMessageReceived);
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

        public GlobalFilterController GlobalFilterController { get; private set; }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                if (value == this.doNotUseShown) return;
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

        public IEnumerable<Widget> Widgets { get; private set; }

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
            if (Widgets == null)
            {
                Widgets = this.widgetRepository.GetAll();
                UpdateWidgets();
            }
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

            var newHash = message.Criteria.GetHashCode();
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
                var key = typeof(StatementModel);
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

        private bool WidgetCommandCanExecute(Widget widget)
        {
            return widget.Clickable;
        }
    }
}