using System.Diagnostics.CodeAnalysis;
using AutoMapper;

namespace BudgetAnalyser.Engine.Persistence
{
    [AutoRegisterWithIoC]
    internal class ApplicationAutoMapperConfiguration : ILocalAutoMapperConfiguration
    {
        /// <summary>
        ///     Register the localised mappings.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Ok in AutoMapper files of this size")]
        public void RegisterMappings()
        {
            Mapper.CreateMap<BudgetAnalyserStorageRoot, ApplicationDatabase>()
                .ForMember(db => db.BudgetCollectionStorageKey, m => m.MapFrom(dto => dto.BudgetCollectionRootDto.Source))
                .ForMember(db => db.LedgerBookStorageKey, m => m.MapFrom(dto => dto.LedgerBookRootDto.Source))
                .ForMember(db => db.MatchingRulesCollectionStorageKey, m => m.MapFrom(dto => dto.MatchingRulesCollectionRootDto.Source))
                .ForMember(db => db.StatementModelStorageKey, m => m.MapFrom(dto => dto.StatementModelRootDto.Source))
                .ForMember(db => db.LedgerReconciliationToDoCollection, m => m.MapFrom(dto => dto.LedgerReconciliationToDoCollection))
                .ForMember(db => db.FileName, m => m.Ignore());

            Mapper.CreateMap<ApplicationDatabase, BudgetAnalyserStorageRoot>()
                .ForMember(dto => dto.BudgetCollectionRootDto, m => m.MapFrom(db => db.BudgetCollectionStorageKey))
                .ForMember(dto => dto.LedgerBookRootDto, m => m.MapFrom(db => db.LedgerBookStorageKey))
                .ForMember(dto => dto.LedgerReconciliationToDoCollection, m => m.MapFrom(db => db.LedgerReconciliationToDoCollection))
                .ForMember(dto => dto.MatchingRulesCollectionRootDto, m => m.MapFrom(db => db.MatchingRulesCollectionStorageKey))
                .ForMember(dto => dto.StatementModelRootDto, m => m.MapFrom(db => db.StatementModelStorageKey));

            Mapper.CreateMap<StorageBranch, string>()
                .ConstructUsing(s => s.Source);

            Mapper.CreateMap<string, StorageBranch>()
                .ForMember(dto => dto.Source, m => m.MapFrom(s => s));
        }
    }
}