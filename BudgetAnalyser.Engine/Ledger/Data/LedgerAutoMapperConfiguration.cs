using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC]
    internal class LedgerAutoMapperConfiguration : ILocalAutoMapperConfiguration
    {
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly ILedgerBookFactory ledgerBookFactory;
        private readonly ILedgerBucketFactory ledgerBucketFactory;
        private readonly ILedgerTransactionFactory ledgerTransactionFactory;
        private readonly ILogger logger;

        public LedgerAutoMapperConfiguration(
            [NotNull] ILedgerTransactionFactory ledgerTransactionFactory,
            [NotNull] IAccountTypeRepository accountTypeRepo,
            [NotNull] ILogger logger,
            [NotNull] ILedgerBookFactory ledgerBookFactory,
            [NotNull] ILedgerBucketFactory ledgerBucketFactory)
        {
            if (ledgerTransactionFactory == null)
            {
                throw new ArgumentNullException(nameof(ledgerTransactionFactory));
            }

            if (accountTypeRepo == null)
            {
                throw new ArgumentNullException(nameof(accountTypeRepo));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (ledgerBucketFactory == null)
            {
                throw new ArgumentNullException(nameof(ledgerBucketFactory));
            }

            this.ledgerTransactionFactory = ledgerTransactionFactory;
            this.accountTypeRepo = accountTypeRepo;
            this.logger = logger;
            this.ledgerBookFactory = ledgerBookFactory;
            this.ledgerBucketFactory = ledgerBucketFactory;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Simple Automapper expressions artificially inflating cyclomatic complexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Necessary for the mapping in this namespace")]
        public void RegisterMappings()
        {
            Mapper.CreateMap<BankBalanceAdjustmentTransaction, LedgerTransactionDto>()
                .ForMember(dto => dto.TransactionType, m => m.MapFrom(transaction => transaction.GetType().FullName))
                .ForMember(dto => dto.Account, m => m.MapFrom(transaction => transaction.BankAccount.Name));

            Mapper.CreateMap<LedgerTransactionDto, BankBalanceAdjustmentTransaction>()
                .ForMember(
                    transaction => transaction.BankAccount,
                    m => m.MapFrom(dto => this.accountTypeRepo.GetByKey(dto.Account) ?? this.accountTypeRepo.GetByKey(AccountTypeRepositoryConstants.Cheque)));

            Mapper.CreateMap<LedgerTransaction, LedgerTransactionDto>()
                .ForMember(dto => dto.TransactionType, m => m.MapFrom(transaction => transaction.GetType().FullName))
                .ForMember(dto => dto.Account, m => m.Ignore());

            Mapper.CreateMap<LedgerTransactionDto, LedgerTransaction>()
                .ConstructUsing(dto => this.ledgerTransactionFactory.Build(dto.TransactionType, dto.Id));

            Mapper.CreateMap<BankBalance, BankBalanceDto>()
                .ForMember(dto => dto.Account, m => m.MapFrom(bankBalance => bankBalance.Account.Name));

            Mapper.CreateMap<string, Account>()
                .ConvertUsing(name => this.accountTypeRepo.GetByKey(name));

            Mapper.CreateMap<BankBalanceDto, BankBalance>()
                .ForMember(
                    balance => balance.Account,
                    m => m.MapFrom(dto => this.accountTypeRepo.GetByKey(dto.Account) ?? this.accountTypeRepo.GetByKey(AccountTypeRepositoryConstants.Cheque)));

            Mapper.CreateMap<LedgerBucket, LedgerBucketDto>()
                .ForMember(dto => dto.BucketCode, m => m.MapFrom(ledger => ledger.BudgetBucket.Code))
                .ForMember(dto => dto.StoredInAccount, m => m.MapFrom(ledger => ledger.StoredInAccount.Name));

            Mapper.CreateMap<LedgerBucketDto, LedgerBucket>()
                .ConvertUsing(dto => this.ledgerBucketFactory.Build(dto.BucketCode, dto.StoredInAccount));

            Mapper.CreateMap<LedgerEntry, LedgerEntryDto>()
                .ForMember(dto => dto.BucketCode, m => m.MapFrom(ledgerEntry => ledgerEntry.LedgerBucket.BudgetBucket.Code))
                .ForMember(dto => dto.StoredInAccount, m => m.MapFrom(ledgerEntry => ledgerEntry.LedgerBucket.StoredInAccount.Name));

            Mapper.CreateMap<LedgerEntryDto, LedgerEntry>()
                .ForMember(entry => entry.Transactions, m => m.MapFrom(dto => dto.Transactions.OrderByDescending(t => t.TransactionType)))
                .ForMember(
                    entry => entry.LedgerBucket,
                    m => m.MapFrom(dto => this.ledgerBucketFactory.Build(dto.BucketCode, dto.StoredInAccount))); 

            Mapper.CreateMap<LedgerEntryLineDto, LedgerEntryLine>()
                .ForMember(line => line.IsNew, m => m.MapFrom(dto => false));

            Mapper.CreateMap<LedgerEntryLine, LedgerEntryLineDto>()
                .ForMember(dto => dto.BankBalance, m => m.MapFrom(line => line.TotalBankBalance));

            Mapper.CreateMap<LedgerBook, LedgerBookDto>()
                .ForMember(dto => dto.Checksum, m => m.Ignore());

            Mapper.CreateMap<LedgerBookDto, LedgerBook>()
                .ConstructUsing(new Func<LedgerBookDto, LedgerBook>(dto => this.ledgerBookFactory.CreateNew()));

            Mapper.CreateMap<ToDoTask, ToDoTaskDto>();

            Mapper.CreateMap<ToDoTaskDto, ToDoTask>();

            this.logger.LogInfo(_ => "Ledger AutoMapper Configuration Mappings Registered.");
        }
    }
}