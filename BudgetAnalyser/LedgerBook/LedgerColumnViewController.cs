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

        public LedgerColumnViewController([NotNull] IAccountTypeRepository accountRepo)
        {
            if (accountRepo == null)
            {
                throw new ArgumentNullException("accountRepo");
            }

            this.accountRepo = accountRepo;
        }

        public ObservableCollection<AccountType> BankAccounts { get; private set; }

        public BudgetBucket BucketBeingTracked { get; private set; }

        public decimal MonthlyBudgetAmount { get; private set; }
        public AccountType StoredInAccount { get; set; }

        public void ShowDialog(LedgerColumn ledgerColumn, BudgetModel budgetModel)
        {
            BankAccounts = new ObservableCollection<AccountType>(this.accountRepo.ListCurrentlyUsedAccountTypes());
            BucketBeingTracked = ledgerColumn.BudgetBucket;
            StoredInAccount = ledgerColumn.StoredInAccount;
            MonthlyBudgetAmount = budgetModel.Expenses.Single(e => e.Bucket == BucketBeingTracked).Amount;

            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.Ok)
            {
                CorrelationId = Guid.NewGuid(),
                Title = "Ledger - " + BucketBeingTracked,
            };

            MessengerInstance.Send(dialogRequest);
        }
    }
}