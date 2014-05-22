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

        public decimal BankBalanceTotal
        {
            get { return BankBalances.Sum(b => b.Balance); }
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

        public DateTime Date
        {
            get { return this.doNotUseDate; }
            set
            {
                this.doNotUseDate = value;
                RaisePropertyChanged(() => Date);
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

        public void ShowDialog()
        {
            Date = DateTime.Today;
            Canceled = false;
            BankAccounts = this.accountTypeRepository.ListCurrentlyUsedAccountTypes();
            SelectedBankAccount = null;
            BankBalances = new ObservableCollection<BankBalance>();

            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "New Reconciliation",
            };
            MessengerInstance.Send(dialogRequest);
        }

        private bool CanExecuteAddBankBalanceCommand()
        {
            return SelectedBankAccount != null && BankBalance > 0;
        }

        private void OnAddBankBalanceCommandExecuted()
        {
            BankBalances.Add(new BankBalance { Account = SelectedBankAccount, Balance = BankBalance });
            SelectedBankAccount = null;
            BankBalance = 0;
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
            }

            if (!BankBalances.Any())
            {
                if (CanExecuteAddBankBalanceCommand())
                {
                    OnAddBankBalanceCommandExecuted();
                }
                else
                {
                    throw new InvalidOperationException("Failed to add any bank balances to LedgerBook reconciliation.");
                }
            }

            EventHandler handler = Complete;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}