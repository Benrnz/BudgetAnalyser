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
        private readonly ILogger logger;

        public BudgetAutoMapperConfiguration(
            [NotNull] IBudgetBucketFactory bucketFactory,
            [NotNull] IBudgetBucketRepository bucketRepo,
            [NotNull] ILogger logger)
        {
            if (bucketFactory == null)
            {
                throw new ArgumentNullException(nameof(bucketFactory));
            }

            if (bucketRepo == null)
            {
                throw new ArgumentNullException(nameof(bucketRepo));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.bucketFactory = bucketFactory;
            BucketRepository = bucketRepo;
            this.logger = logger;
        }

        internal IBudgetBucketRepository BucketRepository { get; }

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
                .ForMember(expense => expense.Bucket, m => m.MapFrom(dto => BucketRepository.GetByCode(dto.BudgetBucketCode)));

            Mapper.CreateMap<Income, IncomeDto>()
                .ForMember(dto => dto.BudgetBucketCode, m => m.MapFrom(expense => expense.Bucket.Code));

            Mapper.CreateMap<IncomeDto, Income>()
                .ForMember(income => income.Bucket, m => m.MapFrom(dto => BucketRepository.GetByCode(dto.BudgetBucketCode)));

            Mapper.CreateMap<BudgetModel, BudgetModelDto>();

            Mapper.CreateMap<BudgetModelDto, BudgetModel>();

            Mapper.CreateMap<BudgetCollection, BudgetCollectionDto>()
                .ForMember(dto => dto.Buckets, m => m.MapFrom(collection => BucketRepository.Buckets))
                .ForMember(dto => dto.Budgets, m => m.ResolveUsing(collection => collection.Select(Mapper.Map<BudgetModel, BudgetModelDto>)));

            Mapper.CreateMap<BudgetCollectionDto, BudgetCollection>()
                .ConstructUsing(dto => new BudgetCollection(dto.Budgets.Select(Mapper.Map<BudgetModelDto, BudgetModel>)));

            this.logger.LogInfo(_ => "Budget AutoMapper Configuration Mappings Registered.");
        }
    }
}