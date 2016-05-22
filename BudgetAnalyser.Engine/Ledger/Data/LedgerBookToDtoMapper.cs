using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC]
    internal partial class Mapper_LedgerBookDto_LedgerBook
    {
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly ILedgerBucketFactory bucketFactory;
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly Dictionary<string, LedgerBucket> cachedLedgers = new Dictionary<string, LedgerBucket>();
        private readonly IReconciliationBuilder reconciliationBuilder;
        private readonly ILedgerTransactionFactory transactionFactory;

        public Mapper_LedgerBookDto_LedgerBook(
            [NotNull] IBudgetBucketRepository bucketRepo,
            [NotNull] IAccountTypeRepository accountTypeRepo,
            [NotNull] ILedgerBucketFactory bucketFactory,
            [NotNull] ILedgerTransactionFactory transactionFactory,
            [NotNull] IReconciliationBuilder reconciliationBuilder)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            if (accountTypeRepo == null) throw new ArgumentNullException(nameof(accountTypeRepo));
            if (bucketFactory == null) throw new ArgumentNullException(nameof(bucketFactory));
            if (transactionFactory == null) throw new ArgumentNullException(nameof(transactionFactory));
            if (reconciliationBuilder == null) throw new ArgumentNullException(nameof(reconciliationBuilder));
            this.bucketRepo = bucketRepo;
            this.accountTypeRepo = accountTypeRepo;
            this.bucketFactory = bucketFactory;
            this.transactionFactory = transactionFactory;
            this.reconciliationBuilder = reconciliationBuilder;
        }

        // ReSharper disable once UnusedParameter.Local - This argument is used to optionally detect elements not in array.
        private LedgerBucket GetOrAddFromCache(LedgerBucket ledger, bool throwIfNotFound = false)
        {
            if (this.cachedLedgers.ContainsKey(ledger.BudgetBucket.Code))
            {
                return this.cachedLedgers[ledger.BudgetBucket.Code];
            }

            if (throwIfNotFound)
            {
                throw new ArgumentException($"Ledger Bucket {ledger.BudgetBucket.Code} not found in cache.");
            }

            this.cachedLedgers.Add(ledger.BudgetBucket.Code, ledger);
            return ledger;
        }

        /// <summary>
        ///     Custom initialisation and validation to be done directly after mapping the <see cref="LedgerBook" />.
        ///     For example: Must make sure that the <see cref="LedgerBook.Ledgers" /> Collection is populated and each one has a
        ///     default storage Account.
        /// </summary>
        private void InitialiseAndValidateLedgerBook(LedgerBookDto dto, LedgerBook model)
        {
            this.cachedLedgers.Clear();
            foreach (var ledgerBucket in model.Ledgers)
            {
                if (ledgerBucket.StoredInAccount == null)
                {
                    // Defaults to Cheque Account if unspecified.
                    ledgerBucket.StoredInAccount = this.accountTypeRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
                }

                GetOrAddFromCache(ledgerBucket);
            }

            var ledgersMapWasEmpty = model.Ledgers.None();

            // Default to CHEQUE when StoredInAccount is null.
            foreach (var line in model.Reconciliations)
            {
                foreach (var entry in line.Entries)
                {
                    // Ensure the ledger bucker is the same instance as listed in the book.Legders;
                    entry.LedgerBucket = GetOrAddFromCache(entry.LedgerBucket, true);
                    if (entry.LedgerBucket.StoredInAccount == null)
                    {
                        entry.LedgerBucket.StoredInAccount = this.accountTypeRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
                    }
                }
            }

            // If ledger column map at the book level was empty, default it to the last used ledger columns in the Dated Entries.
            if (ledgersMapWasEmpty && model.Reconciliations.Any())
            {
                model.Ledgers = model.Reconciliations.First().Entries.Select(e => e.LedgerBucket);
            }
        }

        // ReSharper disable once RedundantAssignment
        partial void ModelFactory(LedgerBookDto dto, ref LedgerBook model)
        {
            // TODO Seems odd that a mapper requires the ReconciliationBuilder, either need a factory or better yet move the reconciliation code out of the LedgerBook class.
            model = new LedgerBook(this.reconciliationBuilder);
        }

        partial void ToModelPostprocessing(LedgerBookDto dto, ref LedgerBook model)
        {
            InitialiseAndValidateLedgerBook(dto, model);
        }
    }

    [AutoRegisterWithIoC]
    internal partial class Mapper_LedgerBucketDto_LedgerBucket
    {
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly ILedgerBucketFactory bucketFactory;
        private readonly IBudgetBucketRepository bucketRepo;

        public Mapper_LedgerBucketDto_LedgerBucket([NotNull] IBudgetBucketRepository bucketRepo, [NotNull] IAccountTypeRepository accountTypeRepo, [NotNull] ILedgerBucketFactory bucketFactory)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            if (accountTypeRepo == null) throw new ArgumentNullException(nameof(accountTypeRepo));
            if (bucketFactory == null) throw new ArgumentNullException(nameof(bucketFactory));
            this.bucketRepo = bucketRepo;
            this.accountTypeRepo = accountTypeRepo;
            this.bucketFactory = bucketFactory;
        }

        // ReSharper disable once RedundantAssignment
        partial void ModelFactory(LedgerBucketDto dto, ref LedgerBucket model)
        {
            model = this.bucketFactory.Build(dto.BucketCode, dto.StoredInAccount);
        }

        partial void ToDtoPostprocessing(ref LedgerBucketDto dto, LedgerBucket model)
        {
            dto.BucketCode = model.BudgetBucket.Code;
            dto.StoredInAccount = model.StoredInAccount.Name;
        }

        partial void ToModelPostprocessing(LedgerBucketDto dto, ref LedgerBucket model)
        {
            model.BudgetBucket = this.bucketRepo.GetByCode(dto.BucketCode);
            model.StoredInAccount = this.accountTypeRepo.GetByKey(dto.StoredInAccount);
        }
    }

    [AutoRegisterWithIoC]
    internal partial class Mapper_LedgerEntryLineDto_LedgerEntryLine
    {
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly ILedgerBucketFactory bucketFactory;
        private readonly ILedgerTransactionFactory transactionFactory;

        public Mapper_LedgerEntryLineDto_LedgerEntryLine(
            [NotNull] IAccountTypeRepository accountTypeRepo,
            [NotNull] ILedgerBucketFactory bucketFactory,
            [NotNull] ILedgerTransactionFactory transactionFactory)
        {
            if (accountTypeRepo == null) throw new ArgumentNullException(nameof(accountTypeRepo));
            if (bucketFactory == null) throw new ArgumentNullException(nameof(bucketFactory));
            if (transactionFactory == null) throw new ArgumentNullException(nameof(transactionFactory));
            this.accountTypeRepo = accountTypeRepo;
            this.bucketFactory = bucketFactory;
            this.transactionFactory = transactionFactory;
        }

        partial void ToDtoPostprocessing(ref LedgerEntryLineDto dto, LedgerEntryLine model)
        {
            dto.BankBalance = model.TotalBankBalance;
        }

        partial void ToModelPostprocessing(LedgerEntryLineDto dto, ref LedgerEntryLine model)
        {
            model.Lock();
        }
    }

    [AutoRegisterWithIoC]
    internal partial class Mapper_LedgerTransactionDto_BankBalanceAdjustmentTransaction
    {
        private readonly IAccountTypeRepository accountTypeRepo;

        public Mapper_LedgerTransactionDto_BankBalanceAdjustmentTransaction([NotNull] IAccountTypeRepository accountTypeRepo)
        {
            if (accountTypeRepo == null) throw new ArgumentNullException(nameof(accountTypeRepo));
            this.accountTypeRepo = accountTypeRepo;
        }

        partial void ToDtoPostprocessing(ref LedgerTransactionDto dto, BankBalanceAdjustmentTransaction model)
        {
            dto.Account = model.BankAccount.Name;
            dto.TransactionType = model.GetType().FullName;
        }

        partial void ToModelPostprocessing(LedgerTransactionDto dto, ref BankBalanceAdjustmentTransaction model)
        {
            model.BankAccount = this.accountTypeRepo.GetByKey(dto.Account) ?? this.accountTypeRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
        }
    }

    [AutoRegisterWithIoC]
    internal partial class Mapper_BankBalanceDto_BankBalance
    {
        private readonly IAccountTypeRepository accountTypeRepo;

        public Mapper_BankBalanceDto_BankBalance([NotNull] IAccountTypeRepository accountTypeRepo)
        {
            if (accountTypeRepo == null) throw new ArgumentNullException(nameof(accountTypeRepo));
            this.accountTypeRepo = accountTypeRepo;
        }

        // ReSharper disable once RedundantAssignment
        partial void ModelFactory(BankBalanceDto dto, ref BankBalance model)
        {
            model = new BankBalance(this.accountTypeRepo.GetByKey(dto.Account), dto.Balance);
        }

        partial void ToDtoPostprocessing(ref BankBalanceDto dto, BankBalance model)
        {
            dto.Account = model.Account.Name;
        }
    }

    [AutoRegisterWithIoC]
    internal partial class Mapper_LedgerEntryDto_LedgerEntry
    {
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly ILedgerBucketFactory bucketFactory;
        private readonly ILedgerTransactionFactory transactionFactory;

        public Mapper_LedgerEntryDto_LedgerEntry(
            [NotNull] ILedgerBucketFactory bucketFactory,
            [NotNull] ILedgerTransactionFactory transactionFactory,
            [NotNull] IAccountTypeRepository accountTypeRepo)
        {
            if (bucketFactory == null) throw new ArgumentNullException(nameof(bucketFactory));
            if (transactionFactory == null) throw new ArgumentNullException(nameof(transactionFactory));
            if (accountTypeRepo == null) throw new ArgumentNullException(nameof(accountTypeRepo));
            this.bucketFactory = bucketFactory;
            this.transactionFactory = transactionFactory;
            this.accountTypeRepo = accountTypeRepo;
        }

        partial void ToDtoPostprocessing(ref LedgerEntryDto dto, LedgerEntry model)
        {
            dto.BucketCode = model.LedgerBucket.BudgetBucket.Code;
            dto.StoredInAccount = model.LedgerBucket.StoredInAccount.Name;
        }

        partial void ToModelPostprocessing(LedgerEntryDto dto, ref LedgerEntry model)
        {
            model.LedgerBucket = this.bucketFactory.Build(dto.BucketCode, dto.StoredInAccount);
        }

        partial void ToModelPreprocessing(LedgerEntryDto dto, LedgerEntry model)
        {
            // Transactions must be done first otherwise balance will be changed by adding transactions and the balance should be read from the Dto.
            var transactionMapper = new Mapper_LedgerTransactionDto_LedgerTransaction(this.transactionFactory, this.accountTypeRepo);
            foreach (var txn in dto.Transactions)
            {
                model.AddTransaction(transactionMapper.ToModel(txn));
            }
        }
    }

    [AutoRegisterWithIoC]
    internal partial class Mapper_LedgerTransactionDto_LedgerTransaction
    {
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly ILedgerTransactionFactory transactionFactory;

        public Mapper_LedgerTransactionDto_LedgerTransaction([NotNull] ILedgerTransactionFactory transactionFactory, [NotNull] IAccountTypeRepository accountTypeRepo)
        {
            if (transactionFactory == null) throw new ArgumentNullException(nameof(transactionFactory));
            if (accountTypeRepo == null) throw new ArgumentNullException(nameof(accountTypeRepo));
            this.transactionFactory = transactionFactory;
            this.accountTypeRepo = accountTypeRepo;
        }

        // ReSharper disable once RedundantAssignment
        partial void ModelFactory(LedgerTransactionDto dto, ref LedgerTransaction model)
        {
            model = this.transactionFactory.Build(dto.TransactionType, dto.Id);
        }

        partial void ToDtoPostprocessing(ref LedgerTransactionDto dto, LedgerTransaction model)
        {
            dto.TransactionType = model.GetType().FullName;
            // Inheritance could be better handled.
            var bankBalanceTransaction = model as BankBalanceAdjustmentTransaction;
            if (bankBalanceTransaction != null)
            {
                dto.Account = bankBalanceTransaction.BankAccount.Name;
            }
        }

        partial void ToModelPostprocessing(LedgerTransactionDto dto, ref LedgerTransaction model)
        {
            // Inheritance could be better handled.
            var balanaceTransaction = model as BankBalanceAdjustmentTransaction;
            if (balanaceTransaction != null)
            {
                balanaceTransaction.BankAccount = this.accountTypeRepo.GetByKey(dto.Account);
            }
        }
    }
}