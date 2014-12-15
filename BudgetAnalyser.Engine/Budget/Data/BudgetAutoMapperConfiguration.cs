using System;
using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC]
    internal class BudgetAutoMapperConfiguration : ILocalAutoMapperConfiguration
    {
        private readonly IBudgetBucketFactory bucketFactory;
        private readonly IBudgetBucketRepository bucketRepo;
        private readonly ILogger logger;

        public BudgetAutoMapperConfiguration(
            [NotNull] IBudgetBucketFactory bucketFactory, 
            [NotNull] IBudgetBucketRepository bucketRepo, 
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

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.bucketFactory = bucketFactory;
            this.bucketRepo = bucketRepo;
            this.logger = logger;
        }

        internal IBudgetBucketRepository BucketRepository { get { return this.bucketRepo; } }

        public void RegisterMappings()
        {
            Mapper.CreateMap<FixedBudgetProjectBucket, FixedBudgetBucketDto>()
                .IncludeBase<BudgetBucket, BudgetBucketDto>()
                .ForMember(dto => dto.Type, m => m.Ignore());

            Mapper.CreateMap<FixedBudgetBucketDto, FixedBudgetProjectBucket>()
                .IncludeBase<BudgetBucketDto, BudgetBucket>()
                .ConstructUsing(dto => (FixedBudgetProjectBucket)this.bucketFactory.Build(dto.Type));

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

            Mapper.CreateMap<BudgetCollection, BudgetCollectionDto>()
                .ForMember(dto => dto.Buckets, m => m.MapFrom(collection => this.bucketRepo.Buckets))
                .ForMember(dto => dto.Budgets, m => m.ResolveUsing(collection => collection.Select(Mapper.Map<BudgetModel, BudgetModelDto>)));

            Mapper.CreateMap<BudgetCollectionDto, BudgetCollection>()
                .ConstructUsing(dto => new BudgetCollection(dto.Budgets.Select(Mapper.Map<BudgetModelDto, BudgetModel>)));

            this.logger.LogInfo(_ => "Budget AutoMapper Configuration Mappings Registered.");
        }
    }
}
