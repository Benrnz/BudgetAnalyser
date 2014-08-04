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
        private readonly ILogger logger;

        public StatementAutoMapperConfiguration(
            [NotNull] ITransactionTypeRepository transactionTypeRepo,
            [NotNull] IAccountTypeRepository accountTypeRepo,
            [NotNull] IBudgetBucketRepository bucketRepo,
            [NotNull] ILogger logger)
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

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.transactionTypeRepo = transactionTypeRepo;
            this.accountTypeRepo = accountTypeRepo;
            this.bucketRepo = bucketRepo;
            this.logger = logger;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Necessary for Statement Automapper configuration.")]
        public void RegisterMappings()
        {
            Mapper.CreateMap<TransactionDto, Transaction>()
                .ForMember(txn => txn.TransactionType, m => m.MapFrom(dto => this.transactionTypeRepo.GetOrCreateNew(dto.TransactionType)))
                .ForMember(txn => txn.AccountType, m => m.MapFrom(dto => this.accountTypeRepo.GetByKey(dto.AccountType)))
                .ForMember(txn => txn.BudgetBucket, m => m.MapFrom(dto => this.bucketRepo.GetByCode(dto.BudgetBucketCode)))
                .ForMember(txn => txn.IsSuspectedDuplicate, m => m.Ignore());

            Mapper.CreateMap<Transaction, TransactionDto>()
                .ForMember(dto => dto.AccountType, m => m.MapFrom(txn => txn.AccountType == null ? null : txn.AccountType.Name))
                .ForMember(dto => dto.TransactionType, m => m.MapFrom(txn => txn.TransactionType == null ? null : txn.TransactionType.Name))
                .ForMember(dto => dto.BudgetBucketCode, m => m.MapFrom(txn => txn.BudgetBucket == null ? null : txn.BudgetBucket.Code));

            Mapper.CreateMap<TransactionSetDto, StatementModel>()
                .ConstructUsing(new Func<TransactionSetDto, StatementModel>(dto => new StatementModel(logger)))
                .ForMember(model => model.AllTransactions, m => m.MapFrom(dto => dto.Transactions))
                .ForMember(model => model.ChangeHash, m => m.Ignore())
                .ForMember(model => model.DurationInMonths, m => m.Ignore())
                .ForMember(model => model.Filtered, m => m.Ignore())
                .AfterMap((dto, model) => model.LoadTransactions(model.AllTransactions));

            Mapper.CreateMap<StatementModel, TransactionSetDto>()
                .ForMember(dto => dto.Checksum, m => m.Ignore())
                .ForMember(dto => dto.VersionHash, m => m.Ignore());


            this.logger.LogInfo(() => "Statement AutoMapper Configuration Mappings Registered.");
        }
    }
}
