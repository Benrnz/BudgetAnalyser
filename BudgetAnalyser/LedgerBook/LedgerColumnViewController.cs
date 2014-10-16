using System;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerColumnViewController : ControllerBase
    {
        private readonly IAccountTypeRepository accountRepo;
        private Guid correlationId;
        private LedgerColumn ledger;
        private Engine.Ledger.LedgerBook ledgerBook;

        public event EventHandler Updated;

        public LedgerColumnViewController([NotNull] IAccountTypeRepository accountRepo, IUiContext uiContext)
        {
            if (accountRepo == null)
            {
                throw new ArgumentNullException("accountRepo");
            }

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
            this.accountRepo = accountRepo;
        }

        public ObservableCollection<AccountType> BankAccounts { get; private set; }

        public BudgetBucket BucketBeingTracked { get; private set; }

        public decimal MonthlyBudgetAmount { get; private set; }
        public AccountType StoredInAccount { get; set; }

        public void ShowDialog(Engine.Ledger.LedgerBook parentLedgerBook, LedgerColumn ledgerColumn, BudgetModel budgetModel)
        {
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

                this.ledgerBook.SetLedgerAccount(this.ledger, StoredInAccount);
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