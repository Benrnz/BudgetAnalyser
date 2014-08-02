using System;
using AutoMapper;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC]
    internal class StatementAutoMapperConfiguration : ILocalAutoMapperConfiguration
    {
        private readonly ITransactionTypeRepository transactionTypeRepo;
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly IBudgetBucketRepository bucketRepo;

        public StatementAutoMapperConfiguration(
            [NotNull] ITransactionTypeRepository transactionTypeRepo, 
            [NotNull] IAccountTypeRepository accountTypeRepo, 
            [NotNull] IBudgetBucketRepository bucketRepo
            )
        {
            if (transactionTypeRepo == null)
            {
                throw new ArgumentNullException("transactionTypeRepo");
            }
            
            if (accountTypeRepo == null)
            {
                throw new ArgumentNullException("accountTypeRepo");
            }
            
            if (bucketRepo == null)
            {
                throw new ArgumentNullException("bucketRepo");
            }

            this.transactionTypeRepo = transactionTypeRepo;
            this.accountTypeRepo = accountTypeRepo;
            this.bucketRepo = bucketRepo;
        }

        public void RegisterMappings()
        {
            Mapper.CreateMap<TransactionDto, Transaction>()
                .ForMember(txn => txn.TransactionType, m => m.MapFrom(dto => this.transactionTypeRepo.GetOrCreateNew(dto.TransactionType)))
                .ForMember(txn => txn.AccountType, m => m.MapFrom(dto => this.accountTypeRepo.GetByKey(dto.AccountType)))
                .ForMember(txn => txn.BudgetBucket, m => m.MapFrom(dto => this.bucketRepo.GetByCode(dto.BudgetBucketCode)));

            Mapper.CreateMap<Transaction, TransactionDto>()
                .ForMember(dto => dto.AccountType, m => m.MapFrom(txn => txn.AccountType == null ? null : txn.AccountType.Name))
                .ForMember(dto => dto.TransactionType, m => m.MapFrom(txn => txn.TransactionType == null ? null : txn.TransactionType.Name))
                .ForMember(dto => dto.BudgetBucketCode, m => m.MapFrom(txn => txn.BudgetBucket == null ? null : txn.BudgetBucket.Code));
        }
    }
}
