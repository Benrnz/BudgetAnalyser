using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AddLedgerReconciliationController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly IUserMessageBox messageBox;
        private Guid dialogCorrelationId;
        private bool doNotUseAddBalanceVisibility;
        private IEnumerable<Account> doNotUseBankAccounts;
        private decimal doNotUseBankBalance;
        private DateTime doNotUseDate;
        private bool doNotUseEditable;
        private Account doNotUseSelectedBankAccount;
        private Engine.Ledger.LedgerBook parentBook;

        public AddLedgerReconciliationController(
            [NotNull] UiContext uiContext,
            [NotNull] IAccountTypeRepository accountTypeRepository)
        {
            this.accountTypeRepository = accountTypeRepository;
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException(nameof(accountTypeRepository));
            }

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
            this.messageBox = uiContext.UserPrompts.MessageBox;
        }

        public event EventHandler<EditBankBalancesEventArgs> Complete;
        public string ActionButtonToolTip => "Add new ledger entry line.";

        public bool AddBalanceVisibility
        {
            get { return this.doNotUseAddBalanceVisibility; }
            private set
            {
                this.doNotUseAddBalanceVisibility = value;
                RaisePropertyChanged();
            }
        }

        [UsedImplicitly]
        public ICommand AddBankBalanceCommand => new RelayCommand(OnAddBankBalanceCommandExecuted, CanExecuteAddBankBalanceCommand);

        public decimal? AdjustedBankBalanceTotal
        {
            get { return AddBalanceVisibility ? default(decimal?) : BankBalances.Sum(b => b.AdjustedBalance); }
        }

        public IEnumerable<Account> BankAccounts
        {
            [UsedImplicitly] get { return this.doNotUseBankAccounts; }
            private set
            {
                this.doNotUseBankAccounts = value;
                RaisePropertyChanged();
            }
        }

        public decimal BankBalance
        {
            get { return this.doNotUseBankBalance; }
            set
            {
                this.doNotUseBankBalance = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => BankBalanceTotal);
                RaisePropertyChanged(() => AdjustedBankBalanceTotal);
            }
        }

        public ObservableCollection<BankBalanceViewModel> BankBalances { get; private set; }
        public decimal BankBalanceTotal => BankBalances.Sum(b => b.Balance);
        public bool Canceled { get; private set; }
        public bool CanExecuteCancelButton => true;

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

        public bool CanExecuteSaveButton => false;
        public string CloseButtonToolTip => "Cancel";
        public bool CreateMode { get; private set; }

        public DateTime Date
        {
            get { return this.doNotUseDate; }
            set
            {
                this.doNotUseDate = value;
                RaisePropertyChanged();
            }
        }

        public bool Editable
        {
            get { return this.doNotUseEditable; }
            private set
            {
                this.doNotUseEditable = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Checks to make sure the <see cref="BankBalances" /> collection contains a balance for every ledger that will be
        ///     included in the reconciliation.
        /// </summary>
        public bool HasRequiredBalances => this.parentBook.Ledgers.All(l => BankBalances.Any(b => b.Account == l.StoredInAccount));

        [UsedImplicitly]
        public ICommand RemoveBankBalanceCommand => new RelayCommand<BankBalanceViewModel>(OnRemoveBankBalanceCommandExecuted, x => Editable);

        public Account SelectedBankAccount
        {
            get { return this.doNotUseSelectedBankAccount; }
            set
            {
                this.doNotUseSelectedBankAccount = value;
                RaisePropertyChanged();
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
                throw new ArgumentNullException(nameof(ledgerBook));
            }

            this.parentBook = ledgerBook;
            BankBalances = new ObservableCollection<BankBalanceViewModel>();
            CreateMode = true;
            AddBalanceVisibility = true;
            Editable = true;

            var requestFilterMessage = new RequestFilterMessage(this);
            MessengerInstance.Send(requestFilterMessage);
            Date = requestFilterMessage.Criteria.EndDate?.AddDays(1) ?? DateTime.Today;

            ShowDialogCommon("New Monthly Reconciliation");
        }

        /// <summary>
        ///     Used to show the bank balances involved in this <see cref="LedgerEntryLine" />.
        ///     Only shows balances at this stage, no editing allowed.
        /// </summary>
        public void ShowViewDialog([NotNull] Engine.Ledger.LedgerBook ledgerBook, [NotNull] LedgerEntryLine line)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException(nameof(ledgerBook));
            }

            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            this.parentBook = ledgerBook;
            Date = line.Date;
            BankBalances = new ObservableCollection<BankBalanceViewModel>(line.BankBalances.Select(b => new BankBalanceViewModel(line, b)));
            CreateMode = false;
            AddBalanceVisibility = false;
            Editable = false; // Bank balances are not editable after creating a new Ledger Line at this stage.

            ShowDialogCommon(Editable ? "Edit Bank Balances" : "Bank Balances");
        }

        private void AddNewBankBalance()
        {
            BankBalances.Add(new BankBalanceViewModel(null, SelectedBankAccount, BankBalance));
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

        private void OnRemoveBankBalanceCommandExecuted(BankBalanceViewModel bankBalance)
        {
            if (bankBalance == null)
            {
                return;
            }

            BankBalances.Remove(bankBalance);
            RaisePropertyChanged(() => BankBalanceTotal);
            RaisePropertyChanged(() => AdjustedBankBalanceTotal);
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
                if (message.Response == ShellDialogButton.Help)
                {
                    this.messageBox.Show(
                        "Use your actual pay day as the date, this signifies the closing of the previous month, and opening a new month on your pay day showing available funds in each ledger for the new month. The date used will also be used to select transactions from the statement to calculate the ledger balance. The date is set from the current date range filter (on the dashboard page), using the day after the end date. Transactions will be found by searching one month prior to the given date, excluding the given date. The transactions are used to show how the current ledger balance is calculated. The bank balance is the balance as at the date given, after your pay has been credited.");
                    return;
                }
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
                handler?.Invoke(this, new EditBankBalancesEventArgs { Canceled = Canceled });
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
            List<Account> accountsToShow = this.accountTypeRepository.ListCurrentlyUsedAccountTypes().ToList();
            BankAccounts = accountsToShow.OrderBy(a => a.Name);
            SelectedBankAccount = null;
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = title,
                HelpAvailable = CreateMode
            };

            MessengerInstance.Send(dialogRequest);
        }
    }
}