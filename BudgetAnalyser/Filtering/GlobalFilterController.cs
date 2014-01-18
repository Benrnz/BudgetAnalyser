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

namespace BudgetAnalyser.Filtering
{
    public class GlobalFilterController : ControllerBase
    {
        private readonly IViewLoader accountView;
        private readonly IViewLoader dateView;
        private readonly IUserMessageBox userMessageBox;
        private AccountType doNotUseSelectedAccountType;
        private FilterMode filterMode;

        public GlobalFilterController([NotNull] IUserMessageBox userMessageBox, [NotNull] IViewLoader dateViewLoader, [NotNull] IViewLoader accountViewLoader)
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
        }

        private enum FilterMode
        {
            Dates,
            AccountType,
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

        public GlobalFilterCriteria Criteria { get; set; }

        public AccountType SelectedAccountType
        {
            get { return this.doNotUseSelectedAccountType; }

            set
            {
                this.doNotUseSelectedAccountType = value;
                RaisePropertyChanged(() => SelectedAccountType);
            }
        }

        public void InitialValues(GlobalFilterCriteria criteria)
        {
            Criteria = criteria ?? new GlobalFilterCriteria();
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
        }
    }
}