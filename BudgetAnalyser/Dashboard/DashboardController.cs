using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Widget;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.Dashboard
{
    public class DashboardController : ControllerBase, IShowableController
    {
        private readonly Dictionary<Type, object> availableDependencies = new Dictionary<Type, object>();
        private readonly IWidgetRepository widgetRepository;
        private IEnumerable<AccountType> currentAccountTypes;
        private bool doNotUseShown;
        // TODO Timer for time based widget updates
        // TODO Style changer for when widget escalate to a different style after updating.
        // TODO Support for medium sized tiles
        // TODO Support for medium tiles with image
        // TODO Support for image changes when widget updates

        public DashboardController(UiContext uiContext, [NotNull] IWidgetRepository widgetRepository)
        {
            if (widgetRepository == null)
            {
                throw new ArgumentNullException("widgetRepository");
            }

            this.widgetRepository = widgetRepository;
            GlobalFilterController = uiContext.GlobalFilterController;

            MessagingGate.Register<StatementReadyMessage>(this, OnStatementReadyMessageReceived);
            MessagingGate.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoadedMessageReceived);
            MessagingGate.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
        }

        public ICommand GlobalAccountTypeFilterCommand
        {
            get { return new RelayCommand<FilterMode>(OnGlobalDateFilterCommandExecute, CanExecuteGlobalFilterCommand); }
        }

        public ICommand GlobalDateFilterCommand
        {
            get { return new RelayCommand<FilterMode>(OnGlobalDateFilterCommandExecute, CanExecuteGlobalFilterCommand); }
        }

        public GlobalFilterController GlobalFilterController { get; private set; }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
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

        public IEnumerable<Widget> Widgets { get; private set; }

        private bool CanExecuteGlobalFilterCommand(FilterMode parameter)
        {
            return true;
        }

        private void OnApplicationStateLoadedMessageReceived(ApplicationStateLoadedMessage message)
        {
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

            bool updated = false;
            if (message.Budgets != null)
            {
                this.availableDependencies[typeof(BudgetCollection)] = message.Budgets;
                updated = true;
            }

            if (message.ActiveBudget != null)
            {
                this.availableDependencies[typeof(BudgetCurrencyContext)] = message.ActiveBudget;
                updated = true;
            }

            if (updated)
            {
                UpdateWidgets(typeof(BudgetCollection), typeof(BudgetCurrencyContext));
            }
        }

        private void OnGlobalDateFilterCommandExecute(FilterMode filterType)
        {
            if (filterType == FilterMode.Dates)
            {
                GlobalFilterController.PromptUserForDates();
            }
            else if (filterType == FilterMode.AccountType)
            {
                GlobalFilterController.PromptUserForAccountType(this.currentAccountTypes);
            }
        }

        private void OnStatementReadyMessageReceived([NotNull] StatementReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (message.StatementModel != null)
            {
                this.currentAccountTypes = message.StatementModel.AccountTypes;
                Type key = typeof(StatementModel);
                this.availableDependencies[key] = message.StatementModel;
                UpdateWidgets(key);
            }
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