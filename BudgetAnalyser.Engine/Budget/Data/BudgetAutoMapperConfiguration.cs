using System;
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

        public void RegisterMappings()
        {
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

            this.logger.LogInfo(() => "Budget AutoMapper Configuration Mappings Registered.");
        }
    }
}
