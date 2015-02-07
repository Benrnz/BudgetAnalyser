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
                .ForMember(db => db.BudgetCollection, m => m.MapFrom(dto => dto.BudgetCollectionRootDto.Source))
                .ForMember(db => db.LedgerBook, m => m.MapFrom(dto => dto.LedgerBookRootDto.Source))
                .ForMember(db => db.MatchingRulesCollection, m => m.MapFrom(dto => dto.MatchingRulesCollectionRootDto.Source))
                .ForMember(db => db.StatementModel, m => m.MapFrom(dto => dto.StatementModelRootDto.Source));
        }
    }
}