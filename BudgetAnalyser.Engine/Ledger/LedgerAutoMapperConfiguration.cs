using System;
using AutoMapper;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC]
    internal class LedgerAutoMapperConfiguration : ILocalAutoMapperConfiguration
    {
        private readonly ILedgerTransactionFactory ledgerTransactionFactory;
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly ILogger logger;

        public LedgerAutoMapperConfiguration(
            [NotNull] ILedgerTransactionFactory ledgerTransactionFactory, 
            [NotNull] IAccountTypeRepository accountTypeRepo, 
            [NotNull] IBudgetBucketRepository bucketRepo, 
            [NotNull] ILogger logger)
        {
            if (ledgerTransactionFactory == null)
            {
                throw new ArgumentNullException("ledgerTransactionFactory");
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

            this.ledgerTransactionFactory = ledgerTransactionFactory;
            this.accountTypeRepo = accountTypeRepo;
            this.bucketRepo = bucketRepo;
            this.logger = logger;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Necessary for the mapping in this namespace")]
        public void RegisterMappings()
        {
            Mapper.CreateMap<LedgerTransaction, LedgerTransactionDto>()
                .ForMember(dto => dto.AccountType, m => m.MapFrom(transaction => transaction.BankAccount.Name))
                .ForMember(dto => dto.TransactionType, m => m.MapFrom(transaction => transaction.GetType().FullName));

            Mapper.CreateMap<LedgerTransactionDto, LedgerTransaction>()
                .ConstructUsing(dto => this.ledgerTransactionFactory.Build(dto.TransactionType, dto.Id))
                .ForMember(transaction => transaction.BankAccount,
                    m => m.MapFrom(dto => this.accountTypeRepo.GetByKey(dto.AccountType) ?? this.accountTypeRepo.GetByKey(AccountTypeRepositoryConstants.Cheque)));

            Mapper.CreateMap<BankBalance, BankBalanceDto>();

            Mapper.CreateMap<string, AccountType>()
                .ConvertUsing(name => this.accountTypeRepo.GetByKey(name));

            Mapper.CreateMap<BankBalanceDto, BankBalance>()
                .ForMember(balance => balance.Account,
                    m => m.MapFrom(dto => this.accountTypeRepo.GetByKey(dto.Account) ?? this.accountTypeRepo.GetByKey(AccountTypeRepositoryConstants.Cheque)));

            Mapper.CreateMap<LedgerEntry, LedgerEntryDto>()
                .ForMember(dto => dto.BucketCode, m => m.MapFrom(ledgerEntry => ledgerEntry.LedgerColumn.BudgetBucket.Code));

            Mapper.CreateMap<LedgerEntryDto, LedgerEntry>()
                .ForMember(entry => entry.LedgerColumn, m => m.MapFrom(dto => new LedgerColumn { BudgetBucket = this.bucketRepo.GetByCode(dto.BucketCode) }));

            Mapper.CreateMap<LedgerEntryLineDto, LedgerEntryLine>()
                .ForMember(line => line.IsNew, m => m.MapFrom(dto => false));

            Mapper.CreateMap<LedgerEntryLine, LedgerEntryLineDto>()
                .ForMember(dto => dto.BankBalance, m => m.MapFrom(line => line.TotalBankBalance));

            Mapper.CreateMap<LedgerBook, LedgerBookDto>()
                .ForMember(dto => dto.Checksum, m => m.Ignore());

            Mapper.CreateMap<LedgerBookDto, LedgerBook>()
                .ConstructUsing(new Func<LedgerBookDto, LedgerBook>(dto => new LedgerBook(this.logger)));

        }
    }
}
