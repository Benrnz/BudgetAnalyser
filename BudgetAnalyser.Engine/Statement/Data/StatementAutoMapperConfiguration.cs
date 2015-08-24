using System;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC]
    internal class StatementAutoMapperConfiguration : ILocalAutoMapperConfiguration
    {
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly ILogger logger;
        private readonly ITransactionTypeRepository transactionTypeRepo;

        public StatementAutoMapperConfiguration(
            [NotNull] ITransactionTypeRepository transactionTypeRepo,
            [NotNull] IAccountTypeRepository accountTypeRepo,
            [NotNull] IBudgetBucketRepository bucketRepo,
            [NotNull] ILogger logger)
        {
            if (transactionTypeRepo == null)
            {
                throw new ArgumentNullException(nameof(transactionTypeRepo));
            }

            if (accountTypeRepo == null)
            {
                throw new ArgumentNullException(nameof(accountTypeRepo));
            }

            if (bucketRepo == null)
            {
                throw new ArgumentNullException(nameof(bucketRepo));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.transactionTypeRepo = transactionTypeRepo;
            this.accountTypeRepo = accountTypeRepo;
            this.bucketRepo = bucketRepo;
            this.logger = logger;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Necessary for Statement Automapper configuration.")]
        public void RegisterMappings()
        {
            Mapper.CreateMap<TransactionDto, Transaction>()
                .ForMember(txn => txn.TransactionType, m => m.MapFrom(dto => this.transactionTypeRepo.GetOrCreateNew(dto.TransactionType)))
                .ForMember(txn => txn.Account, m => m.MapFrom(dto => this.accountTypeRepo.GetByKey(dto.Account)))
                .ForMember(txn => txn.BudgetBucket, m => m.MapFrom(dto => this.bucketRepo.GetByCode(dto.BudgetBucketCode)))
                .ForMember(txn => txn.IsSuspectedDuplicate, m => m.Ignore());

            Mapper.CreateMap<Transaction, TransactionDto>()
                .ForMember(dto => dto.Account, m => m.MapFrom(txn => txn.Account == null ? null : txn.Account.Name))
                .ForMember(dto => dto.TransactionType, m => m.MapFrom(txn => txn.TransactionType == null ? null : txn.TransactionType.Name))
                .ForMember(dto => dto.BudgetBucketCode, m => m.MapFrom(txn => txn.BudgetBucket == null ? null : txn.BudgetBucket.Code));

            Mapper.CreateMap<TransactionSetDto, StatementModel>()
                .ConstructUsing(new Func<TransactionSetDto, StatementModel>(dto => new StatementModel(this.logger)))
                .ForMember(model => model.AllTransactions, m => m.MapFrom(dto => dto.Transactions))
                .ForMember(model => model.DurationInMonths, m => m.Ignore())
                .ForMember(model => model.Filtered, m => m.Ignore())
                .AfterMap((dto, model) => model.LoadTransactions(model.AllTransactions));

            Mapper.CreateMap<StatementModel, TransactionSetDto>()
                .ForMember(dto => dto.Checksum, m => m.Ignore())
                .ForMember(dto => dto.VersionHash, m => m.Ignore())
                .ForMember(dto => dto.Transactions, m => m.MapFrom(model => model.AllTransactions));


            this.logger.LogInfo(_ => "Statement AutoMapper Configuration Mappings Registered.");
        }
    }
}