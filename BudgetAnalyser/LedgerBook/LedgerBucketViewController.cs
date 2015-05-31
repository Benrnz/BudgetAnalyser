using System;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.Messaging;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerBucketViewController : ControllerBase
    {
        private readonly IAccountTypeRepository accountRepo;
        private readonly ILedgerService ledgerService;
        private Guid correlationId;
        private LedgerBucket ledger;
        private Engine.Ledger.LedgerBook ledgerBook;
        private IUserMessageBox messageBox;

        public event EventHandler Updated;

        public LedgerBucketViewController([NotNull] IAccountTypeRepository accountRepo, [NotNull] IUiContext context, [NotNull] ILedgerService ledgerService)
        {
            if (accountRepo == null)
            {
                throw new ArgumentNullException("accountRepo");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (ledgerService == null)
            {
                throw new ArgumentNullException("ledgerService");
            }

            MessengerInstance = context.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
            this.accountRepo = accountRepo;
            this.ledgerService = ledgerService;
            this.messageBox = context.UserPrompts.MessageBox;
        }

        public LedgerBucketHistoryAnalyser LedgerBucketHistoryAnalysis { get; private set; }

        public ObservableCollection<Account> BankAccounts { get; private set; }

        public BudgetBucket BucketBeingTracked { get; private set; }

        public decimal MonthlyBudgetAmount { get; private set; }
        public Account StoredInAccount { get; set; }

        public void ShowDialog([NotNull] Engine.Ledger.LedgerBook parentLedgerBook, [NotNull] LedgerBucket ledgerBucket, [NotNull] BudgetModel budgetModel)
        {
            if (parentLedgerBook == null)
            {
                throw new ArgumentNullException("parentLedgerBook");
            }

            if (ledgerBucket == null)
            {
                throw new ArgumentNullException("ledgerBucket");
            }

            if (budgetModel == null)
            {
                throw new ArgumentNullException("budgetModel");
            }

            if (LedgerBucketHistoryAnalysis == null) LedgerBucketHistoryAnalysis = CreateBucketHistoryAnalyser();
            LedgerBucketHistoryAnalysis.Analyse(ledgerBucket, parentLedgerBook);
            this.ledger = ledgerBucket;
            this.ledgerBook = parentLedgerBook;
            BankAccounts = new ObservableCollection<Account>(this.accountRepo.ListCurrentlyUsedAccountTypes());
            BucketBeingTracked = ledgerBucket.BudgetBucket;
            StoredInAccount = ledgerBucket.StoredInAccount;
            MonthlyBudgetAmount = budgetModel.Expenses.Single(e => e.Bucket == BucketBeingTracked).Amount;
            this.correlationId = Guid.NewGuid();

            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.correlationId,
                Title = "Ledger - " + BucketBeingTracked,
                HelpAvailable = true,
            };

            MessengerInstance.Send(dialogRequest);
        }

        protected virtual LedgerBucketHistoryAnalyser CreateBucketHistoryAnalyser()
        {
            return new LedgerBucketHistoryAnalyser();
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.correlationId))
            {
                return;
            }

            if (message.Response == ShellDialogButton.Cancel)
            {
                return;
            }

            if (message.Response == ShellDialogButton.Help)
            {
                this.messageBox.Show("Ledgers within the Ledger Book track the actual bank balance over time of a single Bucket.  This is especially useful for budget items that you need to save up for. For example, annual vehicle registration, or car maintenance.  It can also be useful to track Spent-Monthly Buckets. Even though they are always spent down to zero each month, (like rent or mortgage payments), sometimes its useful to have an extra payment, for when there are five weekly payments in a month instead of four.");
                return;
            }

            try
            {
                if (this.ledger.StoredInAccount == StoredInAccount)
                {
                    return;
                }

                this.ledgerService.MoveLedgerToAccount(this.ledger, StoredInAccount);
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