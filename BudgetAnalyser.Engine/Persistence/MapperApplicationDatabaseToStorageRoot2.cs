using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Persistence;

[AutoRegisterWithIoC]
public class MapperApplicationDatabaseToStorageRoot2 : IDtoMapper<BudgetAnalyserStorageRoot, ApplicationDatabase>
{
    private readonly IDtoMapper<List<ToDoTaskDto>, ToDoCollection> todoMapper = new MapperToDoCollectionToDto2();

    public BudgetAnalyserStorageRoot ToDto(ApplicationDatabase model)
    {
        var dto = new BudgetAnalyserStorageRoot
        {
            BudgetCollectionRootDto = new StorageBranch { Source = model.BudgetCollectionStorageKey },
            LedgerBookRootDto = new StorageBranch { Source = model.LedgerBookStorageKey },
            StatementModelRootDto = new StorageBranch { Source = model.StatementModelStorageKey },
            LedgerReconciliationToDoCollection = this.todoMapper.ToDto(model.LedgerReconciliationToDoCollection),
            MatchingRulesCollectionRootDto = new StorageBranch { Source = model.MatchingRulesCollectionStorageKey },
            WidgetCollectionRootDto = new StorageBranch { Source = model.WidgetsCollectionStorageKey },
            IsEncrypted = model.IsEncrypted
        };
        return dto;
    }

    public ApplicationDatabase ToModel(BudgetAnalyserStorageRoot dto)
    {
        var baxModel = new ApplicationDatabase
        {
            BudgetCollectionStorageKey = dto.BudgetCollectionRootDto.Source,
            LedgerBookStorageKey = dto.LedgerBookRootDto.Source,
            StatementModelStorageKey = dto.StatementModelRootDto.Source,
            MatchingRulesCollectionStorageKey = dto.MatchingRulesCollectionRootDto.Source,
            IsEncrypted = dto.IsEncrypted,
            LedgerReconciliationToDoCollection = this.todoMapper.ToModel(dto.LedgerReconciliationToDoCollection),
            WidgetsCollectionStorageKey = dto.WidgetCollectionRootDto.Source
        };
        return baxModel;
    }
}
