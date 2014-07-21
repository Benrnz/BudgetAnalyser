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

        public AutoMapperConfiguration([NotNull] IBudgetBucketFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            this.factory = factory;
        }

        public void Configure()
        {
            Mapper.CreateMap<BudgetBucket, BudgetBucketDto>()
                .ForMember(dto => dto.Type, m => m.MapFrom(budgetBucket => this.factory.SerialiseType(budgetBucket)));

            Mapper.CreateMap<BudgetBucketDto, BudgetBucket>().ConstructUsingServiceLocator();
        }
    }
}
