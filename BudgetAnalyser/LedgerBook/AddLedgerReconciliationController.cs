using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AddLedgerReconciliationController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private Guid dialogCorrelationId;
        private bool doNotUseAddBalanceVisibility;
        private IEnumerable<AccountType> doNotUseBankAccounts;
        private decimal doNotUseBankBalance;
        private DateTime doNotUseDate;
        private bool doNotUseEditable;
        private AccountType doNotUseSelectedBankAccount;
        private Engine.Ledger.LedgerBook parentBook;

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

        public event EventHandler<EditBankBalancesEventArgs> Complete;

        public string ActionButtonToolTip
        {
            get { return "Add new ledger entry line."; }
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
                if (CreateMode)
                {
                    return Date != DateTime.MinValue && HasRequiredBalances;
                }

                return Editable && Date != DateTime.MinValue && HasRequiredBalances;
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

        public bool Editable
        {
            get { return this.doNotUseEditable; }
            private set
            {
                this.doNotUseEditable = value;
                RaisePropertyChanged(() => Editable);
            }
        }

        /// <summary>
        ///     Checks to make sure the <see cref="BankBalances" /> collection contains a balance for every ledger that will be
        ///     included in the reconciliation.
        /// </summary>
        public bool HasRequiredBalances
        {
            get
            {
                return this.parentBook.Ledgers.All(l => BankBalances.Any(b => b.Account == l.StoredInAccount));
            }
        }

        public ICommand RemoveBankBalanceCommand
        {
            get { return new RelayCommand<BankBalance>(OnRemoveBankBalanceCommandExecuted, x => Editable); }
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

        /// <summary>
        ///     Used to start a new Ledger Book reconciliation.  This will ultimately add a new <see cref="LedgerEntryLine" /> to
        ///     the <see cref="LedgerBook" />.
        /// </summary>
        public void ShowCreateDialog([NotNull] Engine.Ledger.LedgerBook ledgerBook)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException("ledgerBook");
            }

            this.parentBook = ledgerBook;
            BankBalances = new ObservableCollection<BankBalance>();
            CreateMode = true;
            AddBalanceVisibility = true;
            Editable = true;

            var requestFilterMessage = new RequestFilterMessage(this);
            MessengerInstance.Send(requestFilterMessage);
            Date = requestFilterMessage.Criteria.EndDate == null ? DateTime.Today : requestFilterMessage.Criteria.EndDate.Value.AddDays(1);

            ShowDialogCommon("New Monthly Reconciliation");
        }

        /// <summary>
        ///     Used to show the bank balances involved in this <see cref="LedgerEntryLine" />.
        /// </summary>
        public void ShowEditDialog([NotNull] Engine.Ledger.LedgerBook ledgerBook, [NotNull] LedgerEntryLine line, bool isNewLine)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException("ledgerBook");
            }

            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            this.parentBook = ledgerBook;
            Date = line.Date;
            BankBalances = new ObservableCollection<BankBalance>(line.BankBalances);
            CreateMode = false;
            AddBalanceVisibility = false;
            Editable = false; // Bank balances are not editable after creating a new Ledger Line at this stage.

            ShowDialogCommon(Editable ? "Edit Bank Balances" : "Bank Balances");
        }

        private void AddNewBankBalance()
        {
            BankBalances.Add(new BankBalance(SelectedBankAccount, BankBalance));
            SelectedBankAccount = null;
            BankBalance = 0;
            RaisePropertyChanged(() => HasRequiredBalances);
        }

        private bool CanExecuteAddBankBalanceCommand()
        {
            if (CreateMode)
            {
                return SelectedBankAccount != null && BankBalance > 0;
            }

            if (!Editable)
            {
                return false;
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

        private void OnRemoveBankBalanceCommandExecuted(BankBalance bankBalance)
        {
            if (bankBalance == null)
            {
                return;
            }

            BankBalances.Remove(bankBalance);
            RaisePropertyChanged(() => BankBalanceTotal);
            RaisePropertyChanged(() => HasRequiredBalances);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            try
            {
                if (message.Response == ShellDialogButton.Cancel)
                {
                    Canceled = true;
                }
                else
                {
                    if (BankBalances.None())
                    {
                        if (CanExecuteAddBankBalanceCommand())
                        {
                            // User may have entered some data to add a new bank balance to the collection, but not clicked the add button, instead they just clicked save.
                            OnAddBankBalanceCommandExecuted();
                        }
                        else
                        {
                            throw new InvalidOperationException("Failed to add any bank balances to Ledger Book reconciliation.");
                        }
                    }
                }

                EventHandler<EditBankBalancesEventArgs> handler = Complete;
                if (handler != null)
                {
                    handler(this, new EditBankBalancesEventArgs { Canceled = Canceled });
                }
            }
            finally
            {
                Reset();
            }
        }

        private void Reset()
        {
            this.parentBook = null;
            Date = DateTime.MinValue;
            BankBalances = null;
            BankAccounts = null;
            SelectedBankAccount = null;
        }

        private void ShowDialogCommon(string title)
        {
            Canceled = false;
            List<AccountType> accountsToShow = this.accountTypeRepository.ListCurrentlyUsedAccountTypes().ToList();
            BankAccounts = accountsToShow.OrderBy(a => a.Name);
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