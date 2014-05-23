using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    public class AddLedgerReconciliationController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private Guid dialogCorrelationId;
        private IEnumerable<AccountType> doNotUseBankAccounts;
        private decimal doNotUseBankBalance;
        private DateTime doNotUseDate;
        private AccountType doNotUseSelectedBankAccount;
        private bool doNotUseAddBalanceVisibility;

        public AddLedgerReconciliationController(
            [NotNull] UiContext uiContext,
            [NotNull] IAccountTypeRepository accountTypeRepository)
        {
            this.accountTypeRepository = accountTypeRepository;
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException("accountTypeRepository");
            }

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public event EventHandler Complete;

        public string ActionButtonToolTip
        {
            get { return "Add new ledger entry line."; }
        }

        public ICommand AddBankBalanceCommand
        {
            get { return new RelayCommand(OnAddBankBalanceCommandExecuted, CanExecuteAddBankBalanceCommand); }
        }

        public IEnumerable<AccountType> BankAccounts
        {
            get { return this.doNotUseBankAccounts; }
            private set
            {
                this.doNotUseBankAccounts = value;
                RaisePropertyChanged(() => BankAccounts);
            }
        }

        public decimal BankBalance
        {
            get { return this.doNotUseBankBalance; }
            set
            {
                this.doNotUseBankBalance = value;
                RaisePropertyChanged(() => BankBalance);
                RaisePropertyChanged(() => BankBalanceTotal);
            }
        }

        public decimal BankBalanceTotal
        {
            get { return BankBalances.Sum(b => b.Balance); }
        }

        public ObservableCollection<BankBalance> BankBalances { get; private set; }

        public bool CanExecuteCancelButton
        {
            get { return true; }
        }

        public bool CanExecuteOkButton
        {
            get
            {
                return Date != DateTime.MinValue
                       && (BankBalances.Any() || CanExecuteAddBankBalanceCommand());
            }
        }

        public bool CanExecuteSaveButton
        {
            get { return false; }
        }

        public bool Canceled { get; private set; }

        public string CloseButtonToolTip
        {
            get { return "Cancel"; }
        }

        public bool CreateMode { get; private set; }

        public DateTime Date
        {
            get { return this.doNotUseDate; }
            set
            {
                this.doNotUseDate = value;
                RaisePropertyChanged(() => Date);
            }
        }

        public bool AddBalanceVisibility
        {
            get { return this.doNotUseAddBalanceVisibility; }
            private set
            {
                this.doNotUseAddBalanceVisibility = value;
                RaisePropertyChanged(() => AddBalanceVisibility);
            }
        }

        public ICommand RemoveBankBalance
        {
            get { return new RelayCommand<BankBalance>(OnRemoveBankBalanceCommandExecuted); }
        }

        public AccountType SelectedBankAccount
        {
            get { return this.doNotUseSelectedBankAccount; }
            set
            {
                this.doNotUseSelectedBankAccount = value;
                RaisePropertyChanged(() => SelectedBankAccount);
            }
        }

        public void ShowCreateDialog()
        {
            Date = DateTime.Today;
            BankBalances = new ObservableCollection<BankBalance>();
            CreateMode = true;
            AddBalanceVisibility = true;

            ShowDialogCommon("New Reconciliation");
        }

        public void ShowEditDialog(LedgerEntryLine line)
        {
            Date = line.Date;
            BankBalances = new ObservableCollection<BankBalance>(line.BankBalances);
            CreateMode = false;
            AddBalanceVisibility = false;

            ShowDialogCommon("Edit Bank Balances");
        }

        private bool CanExecuteAddBankBalanceCommand()
        {
            if (CreateMode)
            {
                return SelectedBankAccount != null && BankBalance > 0;
            }

            if (!AddBalanceVisibility)
            {
                return true;
            }

            return SelectedBankAccount != null && BankBalance > 0;
        }

        private void OnAddBankBalanceCommandExecuted()
        {
            if (CreateMode)
            {
                AddNewBankBalance();
                return;
            }

            if (!AddBalanceVisibility)
            {
                AddBalanceVisibility = true;
                return;
            }

            AddBalanceVisibility = false;
            AddNewBankBalance();
        }

        private void AddNewBankBalance()
        {
            this.BankBalances.Add(new BankBalance { Account = this.SelectedBankAccount, Balance = this.BankBalance });
            this.SelectedBankAccount = null;
            this.BankBalance = 0;
        }

        private void OnRemoveBankBalanceCommandExecuted(BankBalance bankBalance)
        {
            if (bankBalance == null)
            {
                return;
            }

            BankBalances.Remove(bankBalance);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (this.dialogCorrelationId != message.CorrelationId)
            {
                return;
            }

            if (message.Response == ShellDialogButton.Cancel)
            {
                Canceled = true;
                return;
            }

            if (!BankBalances.Any())
            {
                if (CanExecuteAddBankBalanceCommand())
                {
                    OnAddBankBalanceCommandExecuted();
                }
                else
                {
                    throw new InvalidOperationException("Failed to add any bank balances to Ledger Book reconciliation.");
                }
            }

            EventHandler handler = Complete;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void ShowDialogCommon(string title)
        {
            Canceled = false;
            BankAccounts = this.accountTypeRepository.ListCurrentlyUsedAccountTypes();
            SelectedBankAccount = null;
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = title,
            };

            MessengerInstance.Send(dialogRequest);
        }
    }
}