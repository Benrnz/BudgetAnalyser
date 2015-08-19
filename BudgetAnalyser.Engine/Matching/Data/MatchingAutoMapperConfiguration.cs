﻿using System;
using AutoMapper;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Matching.Data
{
    [AutoRegisterWithIoC]
    internal class MatchingAutoMapperConfiguration : ILocalAutoMapperConfiguration
    {
        private readonly IMatchingRuleFactory ruleFactory;

        public MatchingAutoMapperConfiguration([NotNull] IMatchingRuleFactory ruleFactory)
        {
            if (ruleFactory == null)
            {
                throw new ArgumentNullException(nameof(ruleFactory));
            }

            this.ruleFactory = ruleFactory;
        }

        public void RegisterMappings()
        {
            Mapper.CreateMap<MatchingRuleDto, MatchingRule>()
                .ConstructUsing(dto => this.ruleFactory.CreateRuleForPersistence(dto.BucketCode))
                .ForMember(rule => rule.Bucket, m => m.Ignore())
                .ForMember(rule => rule.Created, m => m.MapFrom(dto => dto.Created ?? DateTime.Now))
                .ForMember(rule => rule.RuleId, m => m.MapFrom(dto => dto.RuleId ?? Guid.NewGuid()));

            Mapper.CreateMap<MatchingRule, MatchingRuleDto>();

            Mapper.CreateMap<SingleUseMatchingRuleDto, SingleUseMatchingRule>()
                .ConstructUsing(dto => this.ruleFactory.CreateSingleUseRuleForPersistence(dto.BucketCode))
                .ForMember(rule => rule.Bucket, m => m.Ignore())
                .ForMember(rule => rule.Created, m => m.MapFrom(dto => dto.Created ?? DateTime.Now))
                .ForMember(rule => rule.RuleId, m => m.MapFrom(dto => dto.RuleId ?? Guid.NewGuid()));

            Mapper.CreateMap<SingleUseMatchingRule, SingleUseMatchingRuleDto>();
        }
    }
}