using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.Filtering
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class GlobalFilterController : ControllerBase, IShellDialogToolTips
    {
        private readonly IDashboardService dashboardService;
        private readonly IUserMessageBox userMessageBox;
        private Guid dialogCorrelationId;
        private string doNotUseAccountTypeSummary;
        private GlobalFilterCriteria doNotUseCriteria;
        private string doNotUseDateSummaryLine1;
        private string doNotUseDateSummaryLine2;
        private AccountType doNotUseSelectedAccountType;
        private FilterMode filterMode;

        public GlobalFilterController(
            [NotNull] UiContext uiContext,
            [NotNull] IDashboardService dashboardService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (dashboardService == null)
            {
                throw new ArgumentNullException("dashboardService");
            }

            this.dashboardService = dashboardService;
            this.userMessageBox = uiContext.UserPrompts.MessageBox;
            this.doNotUseCriteria = new GlobalFilterCriteria();

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
            MessengerInstance.Register<ApplicationStateLoadFinishedMessage>(this, OnApplicationStateLoadFinished);
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<RequestFilterMessage>(this, OnGlobalFilterRequested);
            MessengerInstance.Register<WidgetActivatedMessage>(this, OnWidgetActivatedMessageReceived);
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
            MessengerInstance.Register<RequestFilterChangeMessage>(this, OnGlobalFilterChangeRequested);
        }

        public IEnumerable<AccountType> AccountTypes { get; private set; }

        public string AccountTypeSummary
        {
            get { return this.doNotUseAccountTypeSummary; }
            private set
            {
                this.doNotUseAccountTypeSummary = value;
                RaisePropertyChanged();
            }
        }

        public string ActionButtonToolTip
        {
            get { return "Apply filter and close."; }
        }

        public ICommand AddMonthCommand
        {
            get { return new RelayCommand<DateTime>(OnAddMonthCommandExecute, d => d != DateTime.MinValue); }
        }

        public ICommand BackMonthCommand
        {
            get { return new RelayCommand<DateTime>(OnBackMonthCommandExecute, d => d != DateTime.MinValue); }
        }

        public ICommand ClearCommand
        {
            get { return new RelayCommand(OnClearCommandExecute); }
        }

        public string CloseButtonToolTip
        {
            get { return "Cancel and do not change the filter."; }
        }

        public GlobalFilterCriteria Criteria
        {
            get { return this.doNotUseCriteria; }
            set
            {
                this.doNotUseCriteria = value;
                RaisePropertyChanged();
                UpdateSummaries();
            }
        }

        public string DateSummaryLine1
        {
            get { return this.doNotUseDateSummaryLine1; }
            private set
            {
                this.doNotUseDateSummaryLine1 = value;
                RaisePropertyChanged();
            }
        }

        public string DateSummaryLine2
        {
            get { return this.doNotUseDateSummaryLine2; }
            private set
            {
                this.doNotUseDateSummaryLine2 = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAccountFilterView
        {
            get { return this.filterMode == FilterMode.AccountType; }
        }

        public bool IsDateFilterView
        {
            get { return this.filterMode == FilterMode.Dates; }
        }

        public AccountType SelectedAccountType
        {
            get { return this.doNotUseSelectedAccountType; }

            set
            {
                this.doNotUseSelectedAccountType = value;
                RaisePropertyChanged();
            }
        }

        public void PromptUserForAccountType()
        {
            this.dialogCorrelationId = Guid.NewGuid();
            this.filterMode = FilterMode.AccountType;

            AccountTypes = this.dashboardService.FilterableAccountTypes();

            SelectedAccountType = Criteria.AccountType;
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Global Filters - Account Type"
            };
            RaisePropertyChanged(() => IsAccountFilterView);
            RaisePropertyChanged(() => IsDateFilterView);
            MessengerInstance.Send(dialogRequest);
        }

        public void PromptUserForDates()
        {
            this.filterMode = FilterMode.Dates;
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Global Filters - Date Range"
            };
            RaisePropertyChanged(() => IsAccountFilterView);
            RaisePropertyChanged(() => IsDateFilterView);
            MessengerInstance.Send(dialogRequest);
        }

        private void OnAddMonthCommandExecute(DateTime date)
        {
            if (Criteria.BeginDate != null && date == Criteria.BeginDate)
            {
                Criteria.BeginDate = Criteria.BeginDate.Value.AddMonths(1);
                return;
            }

            if (Criteria.EndDate != null && date == Criteria.EndDate)
            {
                Criteria.EndDate = Criteria.EndDate.Value.AddMonths(1);
            }
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            var filterState = message.ElementOfType<PersistentFiltersV1>();
            if (filterState == null)
            {
                return;
            }

            Criteria = new GlobalFilterCriteria
            {
                AccountType = filterState.AccountType,
                BeginDate = filterState.BeginDate,
                EndDate = filterState.EndDate
            };

            SendFilterAppliedMessage();
        }

        private void OnApplicationStateLoadFinished(ApplicationStateLoadFinishedMessage message)
        {
            if (Criteria == null || Criteria.Cleared)
            {
                SendFilterAppliedMessage();
            }
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            bool noCriteria = Criteria == null;
            var filterState = new PersistentFiltersV1
            {
                BeginDate = noCriteria ? null : Criteria.BeginDate,
                EndDate = noCriteria ? null : Criteria.EndDate,
                AccountType = noCriteria ? null : Criteria.AccountType
            };

            message.PersistThisModel(filterState);
        }

        private void OnBackMonthCommandExecute(DateTime date)
        {
            if (Criteria.BeginDate != null && date == Criteria.BeginDate)
            {
                Criteria.BeginDate = Criteria.BeginDate.Value.AddMonths(-1);
                return;
            }

            if (Criteria.EndDate != null && date == Criteria.EndDate)
            {
                Criteria.EndDate = Criteria.EndDate.Value.AddMonths(-1);
            }
        }

        private void OnClearCommandExecute()
        {
            switch (this.filterMode)
            {
                case FilterMode.Dates:
                    Criteria.BeginDate = null;
                    Criteria.EndDate = null;
                    break;

                case FilterMode.AccountType:
                    Criteria.AccountType = null;
                    break;
            }
        }

        private void OnGlobalFilterChangeRequested(RequestFilterChangeMessage message)
        {
            Criteria = message.Criteria;
            SendFilterAppliedMessage();
        }

        private void OnGlobalFilterRequested(RequestFilterMessage message)
        {
            message.Criteria = Criteria;
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            if (this.filterMode == FilterMode.AccountType)
            {
                Criteria.AccountType = SelectedAccountType;
            }

            var validationMessages = new StringBuilder();
            if (!Criteria.Validate(validationMessages))
            {
                this.userMessageBox.Show(validationMessages.ToString(), "Invalid Filter Values!");
                return;
            }

            SendFilterAppliedMessage();
        }

        private void OnWidgetActivatedMessageReceived([NotNull] WidgetActivatedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (message.Widget is DateFilterWidget)
            {
                PromptUserForDates();
                return;
            }

            if (message.Widget is AccountFilterWidget)
            {
                PromptUserForAccountType();
            }
        }

        private void SendFilterAppliedMessage()
        {
            UpdateSummaries();
            MessengerInstance.Send(new FilterAppliedMessage(this, Criteria));
        }

        private void UpdateSummaries()
        {
            DateSummaryLine1 = "No date filter applied.";
            DateSummaryLine2 = string.Empty;
            AccountTypeSummary = "No account filter applied.";

            if (Criteria.Cleared)
            {
                return;
            }

            if (Criteria.BeginDate != null)
            {
                DateSummaryLine1 = string.Format(CultureInfo.CurrentCulture, "Filtered from: {0:d}", Criteria.BeginDate.Value);
            }

            if (Criteria.EndDate != null)
            {
                DateSummaryLine2 = string.Format(CultureInfo.CurrentCulture, "up until: {0:d}", Criteria.EndDate.Value);
            }

            if (Criteria.AccountType != null)
            {
                AccountTypeSummary = "Filtered by " + Criteria.AccountType.Name;
            }
        }
    }
}