using System.Collections.Generic;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Persistence
{
    public class BudgetAnalyserStorageRoot
    {
        public BudgetAnalyserStorageRoot()
        {
            LedgerReconciliationToDoCollection = new List<ToDoTaskDto>();
        }

        public StorageBranch BudgetCollectionRootDto { get; set; }
        public StorageBranch LedgerBookRootDto { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<ToDoTaskDto> LedgerReconciliationToDoCollection { get; set; }
        public StorageBranch MatchingRulesCollectionRootDto { get; set; }
        public StorageBranch StatementModelRootDto { get; set; }
    }
}