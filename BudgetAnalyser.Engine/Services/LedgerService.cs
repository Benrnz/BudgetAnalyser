using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
        private readonly IReconciliationConsistency reconciliationConsistency;

        public LedgerService(
            [NotNull] ILedgerBookRepository ledgerRepository,
            [NotNull] IAccountTypeRepository accountTypeRepository,
            [NotNull] ILogger logger,
            [NotNull] ITransactionRuleService transactionRuleService,
            [NotNull] IReconciliationConsistency reconciliationConsistency)
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

            if (reconciliationConsistency == null)
            {
                throw new ArgumentNullException(nameof(reconciliationConsistency));
            }

            this.ledgerRepository = ledgerRepository;
            this.accountTypeRepository = accountTypeRepository;
            this.logger = logger;
            this.transactionRuleService = transactionRuleService;
            this.reconciliationConsistency = reconciliationConsistency;
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
            DateTime reconciliationDate,
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

            try
            {
                PreReconciliationValidation(reconciliationDate, statement);
            }
            catch (ValidationWarningException)
            {
                if (!ignoreWarnings)
                {
                    throw;
                }
            }

            if (ReconciliationToDoList == null)
            {
                ReconciliationToDoList = new ToDoCollection();
            }

            LedgerEntryLine recon;
            using (this.reconciliationConsistency.EnsureConsistency(LedgerBook))
            {
                recon = LedgerBook.Reconcile(reconciliationDate, balances, budgetContext.Model, ReconciliationToDoList, statement);
            }

            foreach (ToDoTask task in ReconciliationToDoList)
            {
                this.logger.LogInfo(l => l.Format("TASK: {0} SystemGenerated:{1}", task.Description, task.SystemGenerated));
                var transferTask = task as TransferTask;
                if (transferTask != null && transferTask.SystemGenerated && transferTask.Reference.IsSomething())
                {
                    this.logger.LogInfo(l => l.Format("TRANSFER-TASK detected- creating new single use rule. SystemGenerated:{1} Reference:{2}", task.Description, task.SystemGenerated, transferTask.Reference));
                    this.transactionRuleService.CreateNewSingleUseRule(transferTask.BucketCode, null, new[] { transferTask.Reference }, null, null, true);
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

        private void PreReconciliationValidation(DateTime reconciliationDate, StatementModel statement)
        {
            var messages = new StringBuilder();
            if (!LedgerBook.Validate(messages))
            {
                throw new InvalidOperationException("Ledger book is currently in an invalid state. Cannot add new entries.\n" + messages);
            }

            if (statement == null)
            {
                return;
            }

            var startDate = ReconciliationBuilder.CalculateDateForReconcile(LedgerBook, reconciliationDate);

            ValidateDates(startDate, reconciliationDate, statement);

            ValidateAgainstUncategorisedTransactions(startDate, reconciliationDate, statement);

            ValidateAgainstOrphanedAutoMatchingTransactions(statement);
        }

        private void ValidateAgainstOrphanedAutoMatchingTransactions(StatementModel statement)
        {
            LedgerEntryLine lastLine = LedgerBook.Reconciliations.FirstOrDefault();
            if (lastLine == null)
            {
                return;
            }

            List<LedgerTransaction> unmatchedTxns = lastLine.Entries
                .SelectMany(e => e.Transactions)
                .Where(t => !string.IsNullOrWhiteSpace(t.AutoMatchingReference) && !t.AutoMatchingReference.StartsWith(ReconciliationBuilder.MatchedPrefix, StringComparison.Ordinal))
                .ToList();

            if (unmatchedTxns.None())
            {
                return;
            }

            List<Transaction> statementSubSet = statement.AllTransactions.Where(t => t.Date >= lastLine.Date).ToList();
            foreach (LedgerTransaction ledgerTransaction in unmatchedTxns)
            {
                IEnumerable<Transaction> statementTxns = ReconciliationBuilder.TransactionsToAutoMatch(statementSubSet, ledgerTransaction.AutoMatchingReference);
                if (statementTxns.None())
                {
                    this.logger.LogWarning(
                        l =>
                            l.Format(
                                "There appears to be some transactions from last month that should be auto-matched to a statement transactions, but no matching statement transactions were found. {0}",
                                ledgerTransaction));
                    throw new ValidationWarningException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "There appears to be some transactions from last month that should be auto-matched to a statement transactions, but no matching statement transactions were found.\nHave you forgotten to do a transfer?\nTransaction ID:{0} Ref:{1} Amount:{2:C}",
                            ledgerTransaction.Id,
                            ledgerTransaction.AutoMatchingReference,
                            ledgerTransaction.Amount));
                }
            }
        }

        private void ValidateAgainstUncategorisedTransactions(DateTime startDate, DateTime reconciliationDate, StatementModel statement)
        {
            if (statement.AllTransactions
                .Where(t => t.Date >= startDate && t.Date < reconciliationDate)
                .Any(t => t.BudgetBucket == null || (t.BudgetBucket != null && string.IsNullOrWhiteSpace(t.BudgetBucket.Code))))
            {
                IEnumerable<Transaction> uncategorised = statement.AllTransactions.Where(t => t.BudgetBucket == null || (t.BudgetBucket != null && string.IsNullOrWhiteSpace(t.BudgetBucket.Code)));
                var count = 0;
                this.logger.LogWarning(_ => "LedgerBook.PreReconciliationValidation: There appears to be transactions in the statement that are not categorised into a budget bucket.");
                foreach (Transaction transaction in uncategorised)
                {
                    count++;
                    Transaction transactionCopy = transaction;
                    this.logger.LogWarning(_ => "LedgerBook.PreReconciliationValidation: Transaction: " + transactionCopy.Id + transactionCopy.BudgetBucket);
                    if (count > 5)
                    {
                        this.logger.LogWarning(_ => "LedgerBook.PreReconciliationValidation: There are more than 5 transactions.");
                    }
                }

                throw new ValidationWarningException("There appears to be transactions in the statement that are not categorised into a budget bucket.");
            }
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void ValidateDates(DateTime startDate, DateTime reconciliationDate, StatementModel statement)
        {
            LedgerEntryLine recentEntry = LedgerBook.Reconciliations.FirstOrDefault();
            if (recentEntry != null)
            {
                if (reconciliationDate <= recentEntry.Date)
                {
                    throw new InvalidOperationException("The start Date entered is before the previous ledger entry.");
                }

                if (recentEntry.Date.AddDays(7 * 4) > reconciliationDate)
                {
                    throw new InvalidOperationException("The start Date entered is not at least 4 weeks after the previous reconciliation. ");
                }

                if (recentEntry.Date.Day != reconciliationDate.Day)
                {
                    throw new ValidationWarningException(
                        "The reconciliation Date chosen, {0}, isn't the same day of the month as the previous entry {1}. Not required, but ideally reconciliations should be evenly spaced.");
                }
            }

            if (!statement.AllTransactions.Any(t => t.Date >= startDate))
            {
                throw new ValidationWarningException("There doesn't appear to be any transactions in the statement for the month up to " + reconciliationDate.ToShortDateString());
            }
        }
    }
}