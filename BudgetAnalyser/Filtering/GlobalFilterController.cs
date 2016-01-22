using System;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;
using ApplicationStateLoadedMessage = BudgetAnalyser.ApplicationState.ApplicationStateLoadedMessage;
using ApplicationStateLoadFinishedMessage = BudgetAnalyser.ApplicationState.ApplicationStateLoadFinishedMessage;
using ApplicationStateRequestedMessage = BudgetAnalyser.ApplicationState.ApplicationStateRequestedMessage;

namespace BudgetAnalyser.Filtering
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class GlobalFilterController : ControllerBase, IShellDialogToolTips
    {
        private readonly IUserMessageBox userMessageBox;
        private Guid dialogCorrelationId;
        private string doNotUseAccountTypeSummary;
        private GlobalFilterCriteria doNotUseCriteria;
        private string doNotUseDateSummaryLine1;
        private string doNotUseDateSummaryLine2;

        public GlobalFilterController(
            [NotNull] UiContext uiContext)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

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

        public string AccountTypeSummary
        {
            [UsedImplicitly]
            get { return this.doNotUseAccountTypeSummary; }
            private set
            {
                this.doNotUseAccountTypeSummary = value;
                RaisePropertyChanged();
            }
        }

        public string ActionButtonToolTip => "Apply filter and close.";

        [UsedImplicitly]
        public ICommand AddMonthCommand => new RelayCommand<DateTime>(OnAddMonthCommandExecute, d => d != DateTime.MinValue);

        [UsedImplicitly]
        public ICommand BackMonthCommand => new RelayCommand<DateTime>(OnBackMonthCommandExecute, d => d != DateTime.MinValue);

        [UsedImplicitly]
        public ICommand ClearCommand => new RelayCommand(OnClearCommandExecute);

        public string CloseButtonToolTip => "Cancel and do not change the filter.";

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
            [UsedImplicitly]
            get { return this.doNotUseDateSummaryLine1; }
            private set
            {
                this.doNotUseDateSummaryLine1 = value;
                RaisePropertyChanged();
            }
        }

        public string DateSummaryLine2
        {
            [UsedImplicitly]
            get { return this.doNotUseDateSummaryLine2; }
            private set
            {
                this.doNotUseDateSummaryLine2 = value;
                RaisePropertyChanged();
            }
        }

        public void PromptUserForDates()
        {
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Global Date Filter"
            };
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
            var filterState = message.ElementOfType<PersistentFiltersApplicationState>();
            if (filterState == null)
            {
                return;
            }

            Criteria = new GlobalFilterCriteria
            {
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
            var filterState = new PersistentFiltersApplicationState
            {
                BeginDate = noCriteria ? null : Criteria.BeginDate,
                EndDate = noCriteria ? null : Criteria.EndDate,
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
            Criteria.BeginDate = null;
            Criteria.EndDate = null;
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
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Widget is DateFilterWidget)
            {
                PromptUserForDates();
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
        }
    }
}