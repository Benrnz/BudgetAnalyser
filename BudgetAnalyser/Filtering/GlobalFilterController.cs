using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.Filtering
{
    public class GlobalFilterController : ControllerBase
    {
        private readonly IViewLoader accountView;
        private readonly IViewLoader dateView;
        private readonly IUserMessageBox userMessageBox;
        private string doNotUseAccountTypeSummary;
        private GlobalFilterCriteria doNotUseCriteria;
        private string doNotUseDateSummaryLine1;
        private string doNotUseDateSummaryLine2;
        private AccountType doNotUseSelectedAccountType;
        private FilterMode filterMode;

        public GlobalFilterController(
            [NotNull] IUserMessageBox userMessageBox,
            [NotNull] IViewLoader dateViewLoader,
            [NotNull] IViewLoader accountViewLoader)
        {
            if (userMessageBox == null)
            {
                throw new ArgumentNullException("userMessageBox");
            }

            if (dateViewLoader == null)
            {
                throw new ArgumentNullException("dateViewLoader");
            }

            if (accountViewLoader == null)
            {
                throw new ArgumentNullException("accountViewLoader");
            }

            this.dateView = dateViewLoader;
            this.accountView = accountViewLoader;
            this.userMessageBox = userMessageBox;
            Criteria = new GlobalFilterCriteria();

            MessagingGate.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
            MessagingGate.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessagingGate.Register<RequestFilterMessage>(this, OnGlobalFilterRequested);
        }

        public string AccountTypeSummary
        {
            get { return this.doNotUseAccountTypeSummary; }
            private set
            {
                this.doNotUseAccountTypeSummary = value;
                RaisePropertyChanged(() => AccountTypeSummary);
            }
        }

        public IEnumerable<AccountType> AccountTypes { get; private set; }

        public ICommand ClearCommand
        {
            get { return new RelayCommand(OnClearCommandExecute); }
        }

        public ICommand CloseCommand
        {
            get { return new RelayCommand(OnCloseCommandExecute); }
        }

        public GlobalFilterCriteria Criteria
        {
            get { return this.doNotUseCriteria; }
            set
            {
                this.doNotUseCriteria = value;
                RaisePropertyChanged(() => Criteria);
                UpdateSummaries();
            }
        }

        public string DateSummaryLine1
        {
            get { return this.doNotUseDateSummaryLine1; }
            private set
            {
                this.doNotUseDateSummaryLine1 = value;
                RaisePropertyChanged(() => DateSummaryLine1);
            }
        }

        public string DateSummaryLine2
        {
            get { return this.doNotUseDateSummaryLine2; }
            private set
            {
                this.doNotUseDateSummaryLine2 = value;
                RaisePropertyChanged(() => DateSummaryLine2);
            }
        }

        public AccountType SelectedAccountType
        {
            get { return this.doNotUseSelectedAccountType; }

            set
            {
                this.doNotUseSelectedAccountType = value;
                RaisePropertyChanged(() => SelectedAccountType);
            }
        }

        public void PromptUserForAccountType(IEnumerable<AccountType> availableAccountTypes)
        {
            this.filterMode = FilterMode.AccountType;
            List<AccountType> accountTypeList = availableAccountTypes.ToList();
            accountTypeList.Insert(0, null);
            AccountTypes = accountTypeList;
            SelectedAccountType = Criteria.AccountType;
            this.accountView.ShowDialog(this);
        }

        public void PromptUserForDates()
        {
            this.filterMode = FilterMode.Dates;
            this.dateView.ShowDialog(this);
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (!message.RehydratedModels.ContainsKey(typeof(PersistentFiltersV1)))
            {
                return;
            }

            var rehydratedFilters = message.RehydratedModels[typeof(PersistentFiltersV1)].AdaptModel<FilterStateV1>();
            Criteria = new GlobalFilterCriteria
            {
                AccountType = rehydratedFilters.AccountType,
                BeginDate = rehydratedFilters.BeginDate,
                EndDate = rehydratedFilters.EndDate,
            };

            SendFilterAppliedMessage();
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            bool noCriteria = Criteria == null;
            var filterState = new PersistentFiltersV1
            {
                Model = new FilterStateV1
                {
                    BeginDate = noCriteria ? null : Criteria.BeginDate,
                    EndDate = noCriteria ? null : Criteria.EndDate,
                    AccountType = noCriteria ? null : Criteria.AccountType,
                },
            };

            message.PersistThisModel(filterState);
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

            this.accountView.Close();
            this.dateView.Close();
        }

        private void OnCloseCommandExecute()
        {
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

            this.accountView.Close();
            this.dateView.Close();
            SendFilterAppliedMessage();
        }

        private void OnGlobalFilterRequested(RequestFilterMessage message)
        {
            message.Criteria = Criteria;
        }

        private void SendFilterAppliedMessage()
        {
            UpdateSummaries();
            Messenger.Send(new FilterAppliedMessage(this, Criteria));
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
                DateSummaryLine1 = "Filtered from: " + Criteria.BeginDate.Value.ToString("dd-MMM-yy");
            }

            if (Criteria.EndDate != null)
            {
                DateSummaryLine2 = "up until: " + Criteria.EndDate.Value.ToString("dd-MMM-yy");
            }

            if (Criteria.AccountType != null)
            {
                AccountTypeSummary = "Filtered by " + Criteria.AccountType.Name;
            }
        }
    }
}