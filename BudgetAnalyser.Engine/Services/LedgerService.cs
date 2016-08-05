using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class LedgerService : ILedgerService, ISupportsModelPersistence
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly ILedgerBucketFactory ledgerBucketFactory;
        private readonly ILedgerBookRepository ledgerRepository;
        private readonly MonitorableDependencies monitorableDependencies;

        public LedgerService(
            [NotNull] ILedgerBookRepository ledgerRepository,
            [NotNull] IAccountTypeRepository accountTypeRepository,
            [NotNull] ILedgerBucketFactory ledgerBucketFactory,
            [NotNull] MonitorableDependencies monitorableDependencies)
        {
            if (ledgerRepository == null)
            {
                throw new ArgumentNullException(nameof(ledgerRepository));
            }

            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException(nameof(accountTypeRepository));
            }

            if (ledgerBucketFactory == null)
            {
                throw new ArgumentNullException(nameof(ledgerBucketFactory));
            }

            if (monitorableDependencies == null) throw new ArgumentNullException(nameof(monitorableDependencies));

            this.ledgerRepository = ledgerRepository;
            this.accountTypeRepository = accountTypeRepository;
            this.ledgerBucketFactory = ledgerBucketFactory;
            this.monitorableDependencies = monitorableDependencies;
        }

        public event EventHandler Closed;
        public event EventHandler NewDataSourceAvailable;
        public event EventHandler Saved;
        public event EventHandler<AdditionalInformationRequestedEventArgs> Saving;
        public event EventHandler<ValidatingEventArgs> Validating;

        public ApplicationDataType DataType => ApplicationDataType.Ledger;

        public LedgerBook LedgerBook { get; private set; }
        public int LoadSequence => 50;

        public void MoveLedgerToAccount(LedgerBucket ledger, Account storedInAccount)
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

        public void RenameLedgerBook(string newName)
        {
            if (newName == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            LedgerBook.Name = newName;
        }

        public LedgerBucket TrackNewBudgetBucket(ExpenseBucket bucket, Account storeInThisAccount)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }
            if (storeInThisAccount == null)
            {
                throw new ArgumentNullException(nameof(storeInThisAccount));
            }

            var newLedger = this.ledgerBucketFactory.Build(bucket.Code, storeInThisAccount);
            return LedgerBook.AddLedger(newLedger);
        }

        public IEnumerable<Account> ValidLedgerAccounts()
        {
            return this.accountTypeRepository.ListCurrentlyUsedAccountTypes();
        }

        public void Close()
        {
            LedgerBook = null;
            Closed?.Invoke(this, EventArgs.Empty);
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

        public async Task LoadAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabase));
            }

            LedgerBook = await this.ledgerRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.LedgerBookStorageKey), applicationDatabase.IsEncrypted);

            this.monitorableDependencies.NotifyOfDependencyChange(LedgerBook);
            NewDataSourceAvailable?.Invoke(this, EventArgs.Empty);
        }

        public async Task SaveAsync(ApplicationDatabase applicationDatabase)
        {
            Saving?.Invoke(this, new AdditionalInformationRequestedEventArgs());

            var messages = new StringBuilder();
            if (!LedgerBook.Validate(messages))
            {
                throw new ValidationWarningException("Ledger Book is invalid, cannot save at this time:\n" + messages);
            }

            await this.ledgerRepository.SaveAsync(LedgerBook, applicationDatabase.FullPath(applicationDatabase.LedgerBookStorageKey), applicationDatabase.IsEncrypted);
            this.monitorableDependencies.NotifyOfDependencyChange(LedgerBook);
            Saved?.Invoke(this, EventArgs.Empty);
        }

        public void SavePreview()
        {
        }

        public bool ValidateModel(StringBuilder messages)
        {
            EventHandler<ValidatingEventArgs> handler = Validating;
            handler?.Invoke(this, new ValidatingEventArgs());

            return LedgerBook.Validate(messages);
        }
    }
}