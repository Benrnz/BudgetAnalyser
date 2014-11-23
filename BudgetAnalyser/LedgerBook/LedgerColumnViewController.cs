using System;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerColumnViewController : ControllerBase
    {
        private readonly IAccountTypeRepository accountRepo;
        private readonly ILedgerService ledgerService;
        private Guid correlationId;
        private LedgerColumn ledger;
        private Engine.Ledger.LedgerBook ledgerBook;

        public event EventHandler Updated;

        public LedgerColumnViewController([NotNull] IAccountTypeRepository accountRepo, [NotNull] IMessenger messenger, [NotNull] ILedgerService ledgerService)
        {
            if (accountRepo == null)
            {
                throw new ArgumentNullException("accountRepo");
            }

            if (messenger == null)
            {
                throw new ArgumentNullException("messenger");
            }

            if (ledgerService == null)
            {
                throw new ArgumentNullException("ledgerService");
            }

            MessengerInstance = messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
            this.accountRepo = accountRepo;
            this.ledgerService = ledgerService;
        }

        public ObservableCollection<AccountType> BankAccounts { get; private set; }

        public BudgetBucket BucketBeingTracked { get; private set; }

        public decimal MonthlyBudgetAmount { get; private set; }
        public AccountType StoredInAccount { get; set; }

        public void ShowDialog([NotNull] Engine.Ledger.LedgerBook parentLedgerBook, [NotNull] LedgerColumn ledgerColumn, [NotNull] BudgetModel budgetModel)
        {
            if (parentLedgerBook == null)
            {
                throw new ArgumentNullException("parentLedgerBook");
            }

            if (ledgerColumn == null)
            {
                throw new ArgumentNullException("ledgerColumn");
            }

            if (budgetModel == null)
            {
                throw new ArgumentNullException("budgetModel");
            }

            this.ledger = ledgerColumn;
            this.ledgerBook = parentLedgerBook;
            BankAccounts = new ObservableCollection<AccountType>(this.accountRepo.ListCurrentlyUsedAccountTypes());
            BucketBeingTracked = ledgerColumn.BudgetBucket;
            StoredInAccount = ledgerColumn.StoredInAccount;
            MonthlyBudgetAmount = budgetModel.Expenses.Single(e => e.Bucket == BucketBeingTracked).Amount;
            this.correlationId = Guid.NewGuid();

            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.correlationId,
                Title = "Ledger - " + BucketBeingTracked,
            };

            MessengerInstance.Send(dialogRequest);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.correlationId))
            {
                return;
            }

            try
            {
                if (message.Response == ShellDialogButton.Cancel)
                {
                    return;
                }

                if (this.ledger.StoredInAccount == StoredInAccount)
                {
                    return;
                }

                this.ledgerService.MoveLedgerToAccount(this.ledgerBook, this.ledger, StoredInAccount);
                var handler = Updated;
                if (handler != null) handler(this, EventArgs.Empty);
            }
            finally
            {
                Reset();
            }
        }

        private void Reset()
        {
            this.ledger = null;
            MonthlyBudgetAmount = 0;
            BankAccounts.Clear();
            StoredInAccount = null;
        }
    }
}