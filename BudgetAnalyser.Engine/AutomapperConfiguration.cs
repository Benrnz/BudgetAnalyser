using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine
{
    public class AutoMapperConfiguration 
    {
        private readonly IBudgetBucketFactory bucketFactory;
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly ILedgerTransactionFactory ledgerTransactionFactory;
        private readonly IAccountTypeRepository accountTypeRepo;
        private readonly ILogger logger;

        public AutoMapperConfiguration(
            [NotNull] IBudgetBucketFactory bucketFactory, 
            [NotNull] IBudgetBucketRepository bucketRepo, 
            [NotNull] ILedgerTransactionFactory ledgerTransactionFactory, 
            [NotNull] IAccountTypeRepository accountTypeRepo, 
            [NotNull] ILogger logger)
        {
            if (bucketFactory == null)
            {
                throw new ArgumentNullException("bucketFactory");
            }

            if (bucketRepo == null)
            {
                throw new ArgumentNullException("bucketRepo");
            }

            if (ledgerTransactionFactory == null)
            {
                throw new ArgumentNullException("ledgerTransactionFactory");
            }

            if (accountTypeRepo == null)
            {
                throw new ArgumentNullException("accountTypeRepo");
            }
            
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.bucketFactory = bucketFactory;
            this.bucketRepo = bucketRepo;
            this.ledgerTransactionFactory = ledgerTransactionFactory;
            this.accountTypeRepo = accountTypeRepo;
            this.logger = logger;
        }

        public AutoMapperConfiguration Configure()
        {
            // Warning! Use of this static mapping configuration has a chance of intermittently failing tests, if the tests are run in parallel (which they are in this project).
            // This may need to change to improve consistency of test results. Failure does seem to be very rare however.

            Mapper.CreateMap<BudgetBucket, BudgetBucketDto>()
                .ForMember(dto => dto.Type, m => m.MapFrom(budgetBucket => this.bucketFactory.SerialiseType(budgetBucket)));

            Mapper.CreateMap<BudgetBucketDto, BudgetBucket>()
                .ConstructUsing(dto => this.bucketFactory.Build(dto.Type));

            Mapper.CreateMap<Expense, ExpenseDto>()
                .ForMember(dto => dto.BudgetBucketCode, m => m.MapFrom(expense => expense.Bucket.Code));

            Mapper.CreateMap<ExpenseDto, Expense>()
                .ForMember(expense => expense.Bucket, m => m.MapFrom(dto => this.bucketRepo.GetByCode(dto.BudgetBucketCode)));

            Mapper.CreateMap<Income, IncomeDto>()
                .ForMember(dto => dto.BudgetBucketCode, m => m.MapFrom(expense => expense.Bucket.Code));

            Mapper.CreateMap<IncomeDto, Income>()
                .ForMember(income => income.Bucket, m => m.MapFrom(dto => this.bucketRepo.GetByCode(dto.BudgetBucketCode)));

            Mapper.CreateMap<BudgetModel, BudgetModelDto>();

            Mapper.CreateMap<BudgetModelDto, BudgetModel>();

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
                //.AfterMap((dto, entry) => entry.SetTransactions(Mapper.Map<IEnumerable<LedgerTransaction>>(dto.Transactions).ToList()));

            Mapper.CreateMap<LedgerEntryLineDto, LedgerEntryLine>();

            Mapper.CreateMap<LedgerEntryLine, LedgerEntryLineDto>()
                .ForMember(dto => dto.BankBalance, m => m.MapFrom(line => line.TotalBankBalance));

            Mapper.CreateMap<LedgerBook, LedgerBookDto>()
                .ForMember(dto => dto.Checksum, m => m.Ignore());

            Mapper.CreateMap<LedgerBookDto, LedgerBook>()
                .ConstructUsing(new Func<LedgerBookDto, LedgerBook>(dto => new LedgerBook(this.logger)));

            return this;
        }
    }
}
