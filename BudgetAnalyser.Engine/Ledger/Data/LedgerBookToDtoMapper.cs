using System;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    internal partial class Mapper_LedgerBookDto_LedgerBook
    {
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly ILedgerBucketFactory bucketFactory;
        private readonly ILedgerTransactionFactory transactionFactory;

        public Mapper_LedgerBookDto_LedgerBook(
            [NotNull] IBudgetBucketRepository bucketRepo, 
            [NotNull] IAccountTypeRepository accountTypeRepo, 
            [NotNull] ILedgerBucketFactory bucketFactory, 
            [NotNull] ILedgerTransactionFactory transactionFactory)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            if (accountTypeRepo == null) throw new ArgumentNullException(nameof(accountTypeRepo));
            if (bucketFactory == null) throw new ArgumentNullException(nameof(bucketFactory));
            if (transactionFactory == null) throw new ArgumentNullException(nameof(transactionFactory));
            this.bucketRepo = bucketRepo;
            this.accountTypeRepo = accountTypeRepo;
            this.bucketFactory = bucketFactory;
            this.transactionFactory = transactionFactory;
        }
    }

    internal partial class Mapper_LedgerBucketDto_LedgerBucket
    {
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly IAccountTypeRepository accountTypeRepo;

        public Mapper_LedgerBucketDto_LedgerBucket([NotNull] IBudgetBucketRepository bucketRepo, [NotNull] IAccountTypeRepository accountTypeRepo)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            if (accountTypeRepo == null) throw new ArgumentNullException(nameof(accountTypeRepo));
            this.bucketRepo = bucketRepo;
            this.accountTypeRepo = accountTypeRepo;
        }

        partial void ToModelPostprocessing(LedgerBucketDto dto, ref LedgerBucket model)
        {
            model.BudgetBucket = this.bucketRepo.GetByCode(dto.BucketCode);
            model.StoredInAccount = this.accountTypeRepo.GetByKey(dto.StoredInAccount);
        }

        partial void ToDtoPostprocessing(ref LedgerBucketDto dto, LedgerBucket model)
        {
            dto.BucketCode = model.BudgetBucket.Code;
            dto.StoredInAccount = model.StoredInAccount.Name;
        }
    }

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
    }

    internal partial class Mapper_LedgerTransactionDto_BankBalanceAdjustmentTransaction
    {
        private readonly IAccountTypeRepository accountTypeRepo;

        public Mapper_LedgerTransactionDto_BankBalanceAdjustmentTransaction([NotNull] IAccountTypeRepository accountTypeRepo)
        {
            if (accountTypeRepo == null) throw new ArgumentNullException(nameof(accountTypeRepo));
            this.accountTypeRepo = accountTypeRepo;
        }

        partial void ToModelPostprocessing(LedgerTransactionDto dto, ref BankBalanceAdjustmentTransaction model)
        {
            model.BankAccount = this.accountTypeRepo.GetByKey(dto.Account) ?? this.accountTypeRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
        }

        partial void ToDtoPostprocessing(ref LedgerTransactionDto dto, BankBalanceAdjustmentTransaction model)
        {
            dto.Account = model.BankAccount.Name;
            dto.TransactionType = model.GetType().FullName;
        }
    }

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

    internal partial class Mapper_LedgerEntryDto_LedgerEntry
    {
        private readonly ILedgerBucketFactory bucketFactory;
        private readonly ILedgerTransactionFactory transactionFactory;

        public Mapper_LedgerEntryDto_LedgerEntry([NotNull] ILedgerBucketFactory bucketFactory, [NotNull] ILedgerTransactionFactory transactionFactory)
        {
            if (bucketFactory == null) throw new ArgumentNullException(nameof(bucketFactory));
            if (transactionFactory == null) throw new ArgumentNullException(nameof(transactionFactory));
            this.bucketFactory = bucketFactory;
            this.transactionFactory = transactionFactory;
        }

        partial void ToModelPostprocessing(LedgerEntryDto dto, ref LedgerEntry model)
        {
            model.LedgerBucket = this.bucketFactory.Build(dto.BucketCode, dto.StoredInAccount);
        }

        partial void ToDtoPostprocessing(ref LedgerEntryDto dto, LedgerEntry model)
        {
            dto.BucketCode = model.LedgerBucket.BudgetBucket.Code;
            dto.StoredInAccount = model.LedgerBucket.StoredInAccount.Name;
        }
    }

    internal partial class Mapper_LedgerTransactionDto_LedgerTransaction
    {
        private readonly ILedgerTransactionFactory transactionFactory;

        public Mapper_LedgerTransactionDto_LedgerTransaction([NotNull] ILedgerTransactionFactory transactionFactory)
        {
            if (transactionFactory == null) throw new ArgumentNullException(nameof(transactionFactory));
            this.transactionFactory = transactionFactory;
        }

        partial void ToDtoPostprocessing(ref LedgerTransactionDto dto, LedgerTransaction model)
        {
            dto.TransactionType = model.GetType().FullName;
        }

        // ReSharper disable once RedundantAssignment
        partial void ModelFactory(LedgerTransactionDto dto, ref LedgerTransaction model)
        {
            model = this.transactionFactory.Build(dto.TransactionType, dto.Id);
        }
    }
}