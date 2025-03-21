using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Persistence;

[AutoRegisterWithIoC]
public class MapperApplicationDatabaseToStorageRoot3 : IDtoMapper<BudgetAnalyserStorageRoot2, ApplicationDatabase>
{
    private readonly IDtoMapper<List<ToDoTaskDto>, ToDoCollection> todoMapper = new MapperToDoCollectionToDto2();

    public BudgetAnalyserStorageRoot2 ToDto(ApplicationDatabase model)
    {
        var dto = new BudgetAnalyserStorageRoot2
        {
            BudgetCollectionRootDto = model.BudgetCollectionStorageKey,
            LedgerBookRootDto = model.LedgerBookStorageKey,
            StatementModelRootDto = model.StatementModelStorageKey,
            LedgerReconciliationToDoCollection = this.todoMapper.ToDto(model.LedgerReconciliationToDoCollection),
            MatchingRulesCollectionRootDto = model.MatchingRulesCollectionStorageKey,
            WidgetCollectionRootDto = model.WidgetsCollectionStorageKey,
            IsEncrypted = model.IsEncrypted
        };
        return dto;
    }

    public ApplicationDatabase ToModel(BudgetAnalyserStorageRoot2 dto)
    {
        var baxModel = new ApplicationDatabase
        {
            BudgetCollectionStorageKey = dto.BudgetCollectionRootDto,
            LedgerBookStorageKey = dto.LedgerBookRootDto,
            StatementModelStorageKey = dto.StatementModelRootDto,
            MatchingRulesCollectionStorageKey = dto.MatchingRulesCollectionRootDto,
            IsEncrypted = dto.IsEncrypted,
            LedgerReconciliationToDoCollection = this.todoMapper.ToModel(dto.LedgerReconciliationToDoCollection),
            WidgetsCollectionStorageKey = dto.WidgetCollectionRootDto
        };
        return baxModel;
    }
}
