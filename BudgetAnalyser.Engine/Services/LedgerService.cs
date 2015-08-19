using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerService : ILedgerService, ISupportsModelPersistence
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly ILedgerBookRepository ledgerRepository;
        private readonly ILogger logger;
        private readonly ITransactionRuleService transactionRuleService;

        public LedgerService(
            [NotNull] ILedgerBookRepository ledgerRepository,
            [NotNull] IAccountTypeRepository accountTypeRepository,
            [NotNull] ILogger logger,
            [NotNull] ITransactionRuleService transactionRuleService)
        {
            if (ledgerRepository == null)
            {
                throw new ArgumentNullException(nameof(ledgerRepository));
            }

            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException(nameof(accountTypeRepository));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (transactionRuleService == null)
            {
                throw new ArgumentNullException(nameof(transactionRuleService));
            }

            this.ledgerRepository = ledgerRepository;
            this.accountTypeRepository = accountTypeRepository;
            this.logger = logger;
            this.transactionRuleService = transactionRuleService;
        }

        public event EventHandler Closed;
        public event EventHandler NewDataSourceAvailable;
        public event EventHandler Saved;
        public event EventHandler<AdditionalInformationRequestedEventArgs> Saving;
        public event EventHandler<ValidatingEventArgs> Validating;
        public ApplicationDataType DataType => ApplicationDataType.Ledger;
        public LedgerBook LedgerBook { get; private set; }
        public int LoadSequence => 50;

        /// <summary>
        /// The To Do List loaded from a persistent storage.
        /// </summary>
        public ToDoCollection ReconciliationToDoList { get; private set; }

        public void CancelBalanceAdjustment(LedgerEntryLine entryLine, Guid transactionId)
        {
            if (entryLine == null)
            {
                throw new ArgumentNullException(nameof(entryLine));
            }

            if (LedgerBook.Reconciliations.All(l => l != entryLine))
            {
                throw new ArgumentException("Ledger Entry Line provided does not exist in the current Ledger Book.", nameof(entryLine));
            }

            entryLine.CancelBalanceAdjustment(transactionId);
        }

        public void Close()
        {
            LedgerBook = null;
            ReconciliationToDoList = new ToDoCollection();
            EventHandler handler = Closed;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public async Task CreateAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase.LedgerBookStorageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(applicationDatabase));
            }

            await this.ledgerRepository.CreateNewAndSaveAsync(applicationDatabase.LedgerBookStorageKey);
            await LoadAsync(applicationDatabase);
        }

        public LedgerTransaction CreateBalanceAdjustment(LedgerEntryLine entryLine, decimal amount, string narrative, Account.Account account)
        {
            if (entryLine == null)
            {
                throw new ArgumentNullException(nameof(entryLine));
            }

            if (narrative == null)
            {
                throw new ArgumentNullException(nameof(narrative));
            }

            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (LedgerBook.Reconciliations.All(l => l != entryLine))
            {
                throw new ArgumentException("Ledger Entry Line provided does not exist in the current Ledger Book.", nameof(entryLine));
            }

            BankBalanceAdjustmentTransaction adjustmentTransaction = entryLine.BalanceAdjustment(amount, narrative).WithAccount(account);
            adjustmentTransaction.Date = entryLine.Date;
            return adjustmentTransaction;
        }

        public LedgerTransaction CreateLedgerTransaction(LedgerEntry ledgerEntry, decimal amount, string narrative)
        {
            if (ledgerEntry == null)
            {
                throw new ArgumentNullException(nameof(ledgerEntry));
            }

            if (narrative == null)
            {
                throw new ArgumentNullException(nameof(narrative));
            }

            if (LedgerBook.Reconciliations.First().Entries.All(e => e != ledgerEntry))
            {
                throw new ArgumentException("Ledger Entry provided does not exist in the current Ledger Book.", nameof(ledgerEntry));
            }

            LedgerTransaction newTransaction = new CreditLedgerTransaction();
            newTransaction.WithAmount(amount).WithNarrative(narrative);
            newTransaction.Date = LedgerBook.Reconciliations.First().Date;
            ledgerEntry.AddTransaction(newTransaction);
            return newTransaction;
        }

        public async Task LoadAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabase));
            }

            ReconciliationToDoList = applicationDatabase.LedgerReconciliationToDoCollection;
            LedgerBook = await this.ledgerRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.LedgerBookStorageKey));

            EventHandler handler = NewDataSourceAvailable;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public LedgerEntryLine MonthEndReconciliation(
            DateTime reconciliationStartDate,
            IEnumerable<BankBalance> balances,
            IBudgetCurrencyContext budgetContext,
            StatementModel statement,
            bool ignoreWarnings = false)
        {
            if (balances == null)
            {
                throw new ArgumentNullException(nameof(balances));
            }

            if (budgetContext == null)
            {
                throw new ArgumentNullException(nameof(budgetContext));
            }

            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (!budgetContext.BudgetActive)
            {
                throw new InvalidOperationException("Reconciling against an inactive budget is invalid.");
            }

            ReconciliationToDoList.Clear();
            Stopwatch stopWatch = Stopwatch.StartNew();
            this.logger.LogInfo(l => l.Format("Starting Ledger Book reconciliation {0}", DateTime.Now));
            LedgerEntryLine recon = LedgerBook.Reconcile(reconciliationStartDate, balances, budgetContext.Model, ReconciliationToDoList, statement, ignoreWarnings);
            foreach (ToDoTask task in ReconciliationToDoList)
            {
                this.logger.LogInfo(l => l.Format("TASK: {0} SystemGenerated:{1}", task.Description, task.SystemGenerated));
                var transferTask = task as TransferTask;
                if (transferTask != null && transferTask.SystemGenerated && transferTask.Reference.IsSomething())
                {
                    this.transactionRuleService.CreateNewSingleUseRule(transferTask.BucketCode, null, new[] { transferTask.Reference }, null, transferTask.Amount, true);
                }
            }

            stopWatch.Stop();
            this.logger.LogInfo(l => l.Format("Finished Ledger Book reconciliation {0}. It took {1:F0}ms", DateTime.Now, stopWatch.ElapsedMilliseconds));
            return recon;
        }

        public void MoveLedgerToAccount(LedgerBucket ledger, Account.Account storedInAccount)
        {
            if (ledger == null)
            {
                throw new ArgumentNullException(nameof(ledger));
            }
            if (storedInAccount == null)
            {
                throw new ArgumentNullException(nameof(storedInAccount));
            }

            LedgerBook.SetLedgerAccount(ledger, storedInAccount);
        }

        public void RemoveReconciliation(LedgerEntryLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            LedgerBook.RemoveLine(line);
        }

        public void RemoveTransaction(LedgerEntry ledgerEntry, Guid transactionId)
        {
            if (ledgerEntry == null)
            {
                throw new ArgumentNullException(nameof(ledgerEntry));
            }

            if (LedgerBook.Reconciliations.First().Entries.All(e => e != ledgerEntry))
            {
                throw new ArgumentException("Ledger Entry provided does not exist in the current Ledger Book.", nameof(ledgerEntry));
            }

            ledgerEntry.RemoveTransaction(transactionId);
        }

        public void RenameLedgerBook(string newName)
        {
            if (newName == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            LedgerBook.Name = newName;
        }

        public async Task SaveAsync(IReadOnlyDictionary<ApplicationDataType, object> contextObjects)
        {
            EventHandler<AdditionalInformationRequestedEventArgs> savingHandler = Saving;
            savingHandler?.Invoke(this, new AdditionalInformationRequestedEventArgs());

            var messages = new StringBuilder();
            if (!LedgerBook.Validate(messages))
            {
                throw new ValidationWarningException("Ledger Book is invalid, cannot save at this time:\n" + messages);
            }

            await this.ledgerRepository.SaveAsync(LedgerBook, LedgerBook.FileName);
            EventHandler handler = Saved;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public void SavePreview(IDictionary<ApplicationDataType, object> contextObjects)
        {
        }

        public LedgerBucket TrackNewBudgetBucket(ExpenseBucket bucket, Account.Account storeInThisAccount)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }
            if (storeInThisAccount == null)
            {
                throw new ArgumentNullException(nameof(storeInThisAccount));
            }

            return LedgerBook.AddLedger(bucket, storeInThisAccount);
        }

        public LedgerEntryLine UnlockCurrentMonth()
        {
            return LedgerBook.UnlockMostRecentLine();
        }

        public void UpdateRemarks(LedgerEntryLine entryLine, string remarks)
        {
            if (entryLine == null)
            {
                throw new ArgumentNullException(nameof(entryLine));
            }

            if (remarks == null)
            {
                throw new ArgumentNullException(nameof(remarks));
            }

            if (LedgerBook.Reconciliations.All(l => l != entryLine))
            {
                throw new ArgumentException("Ledger Entry Line provided does not exist in the current Ledger Book.", nameof(entryLine));
            }

            entryLine.UpdateRemarks(remarks);
        }

        public bool ValidateModel(StringBuilder messages)
        {
            EventHandler<ValidatingEventArgs> handler = Validating;
            handler?.Invoke(this, new ValidatingEventArgs());

            return LedgerBook.Validate(messages);
        }

        public IEnumerable<Account.Account> ValidLedgerAccounts()
        {
            return this.accountTypeRepository.ListCurrentlyUsedAccountTypes();
        }
    }
}