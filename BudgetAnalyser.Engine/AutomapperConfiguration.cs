using System;
using AutoMapper;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.Engine
{
    public class AutoMapperConfiguration 
    {
        private readonly IBudgetBucketFactory factory;
        private readonly IBudgetBucketRepository bucketRepo;

        public AutoMapperConfiguration([NotNull] IBudgetBucketFactory factory, [NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            if (bucketRepo == null)
            {
                throw new ArgumentNullException("bucketRepo");
            }

            this.factory = factory;
            this.bucketRepo = bucketRepo;
        }

        public void Configure()
        {
            Mapper.CreateMap<BudgetBucket, BudgetBucketDto>()
                .ForMember(dto => dto.Type, m => m.MapFrom(budgetBucket => this.factory.SerialiseType(budgetBucket)));

            Mapper.CreateMap<BudgetBucketDto, BudgetBucket>().ConstructUsing(dto => this.factory.Build(dto.Type));

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
        }
    }
}
