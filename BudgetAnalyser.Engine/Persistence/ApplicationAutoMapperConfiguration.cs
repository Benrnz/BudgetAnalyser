using AutoMapper;

namespace BudgetAnalyser.Engine.Persistence
{
    [AutoRegisterWithIoC]
    internal class ApplicationAutoMapperConfiguration : ILocalAutoMapperConfiguration
    {
        /// <summary>
        /// Register the localised mappings.
        /// </summary>
        public void RegisterMappings()
        {
            Mapper.CreateMap<BudgetAnalyserStorageRoot, ApplicationDatabase>()
                .ForMember(db => db.BudgetCollectionStorageKey, m => m.MapFrom(dto => dto.BudgetCollectionRootDto.Source))
                .ForMember(db => db.LedgerBookStorageKey, m => m.MapFrom(dto => dto.LedgerBookRootDto.Source))
                .ForMember(db => db.MatchingRulesCollectionStorageKey, m => m.MapFrom(dto => dto.MatchingRulesCollectionRootDto.Source))
                .ForMember(db => db.StatementModelStorageKey, m => m.MapFrom(dto => dto.StatementModelRootDto.Source));
        }
    }
}