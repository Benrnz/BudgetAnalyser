using System;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC]
    internal partial class Mapper_TransactionDto_Transaction
    {
        private readonly IAccountTypeRepository accountRepo;
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly ITransactionTypeRepository transactionTypeRepo;

        public Mapper_TransactionDto_Transaction([NotNull] IAccountTypeRepository accountRepo, [NotNull] IBudgetBucketRepository bucketRepo, [NotNull] ITransactionTypeRepository transactionTypeRepo)
        {
            if (accountRepo == null) throw new ArgumentNullException(nameof(accountRepo));
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            if (transactionTypeRepo == null) throw new ArgumentNullException(nameof(transactionTypeRepo));
            this.accountRepo = accountRepo;
            this.bucketRepo = bucketRepo;
            this.transactionTypeRepo = transactionTypeRepo;
        }

        partial void ToDtoPostprocessing(ref TransactionDto dto, Transaction model)
        {
            dto.Account = model.Account.Name;
            dto.BudgetBucketCode = model.BudgetBucket?.Code;
            dto.TransactionType = model.TransactionType.Name;
        }

        partial void ToModelPostprocessing(TransactionDto dto, ref Transaction model)
        {
            model.Account = this.accountRepo.GetByKey(dto.Account);
            model.BudgetBucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode);
            model.TransactionType = this.transactionTypeRepo.GetOrCreateNew(dto.TransactionType);
        }
    }
}