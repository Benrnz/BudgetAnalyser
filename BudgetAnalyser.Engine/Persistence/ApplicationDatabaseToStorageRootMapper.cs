using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Persistence
{
    internal partial class Mapper_BudgetAnalyserStorageRoot_ApplicationDatabase
    {
        partial void ToDtoPostprocessing(ref BudgetAnalyserStorageRoot dto, ApplicationDatabase model)
        {
            dto.BudgetCollectionRootDto = new StorageBranch { Source = model.BudgetCollectionStorageKey };
            dto.LedgerBookRootDto = new StorageBranch { Source = model.LedgerBookStorageKey };
            var mapper = new Mapper_ListToDoTaskDto_ToDoCollection();
            dto.LedgerReconciliationToDoCollection = mapper.ToDto(model.LedgerReconciliationToDoCollection);
            dto.MatchingRulesCollectionRootDto = new StorageBranch { Source = model.MatchingRulesCollectionStorageKey };
            dto.StatementModelRootDto = new StorageBranch { Source = model.StatementModelStorageKey };
        }

        partial void ToModelPostprocessing(BudgetAnalyserStorageRoot dto, ref ApplicationDatabase model)
        {
            model.BudgetCollectionStorageKey = dto.BudgetCollectionRootDto.Source;
            model.LedgerBookStorageKey = dto.LedgerBookRootDto.Source;
            var taskMapper = new Mapper_ListToDoTaskDto_ToDoCollection();
            model.LedgerReconciliationToDoCollection = taskMapper.ToModel(dto.LedgerReconciliationToDoCollection);
            model.MatchingRulesCollectionStorageKey = dto.MatchingRulesCollectionRootDto.Source;
            model.StatementModelStorageKey = dto.StatementModelRootDto.Source;
        }
    }
}