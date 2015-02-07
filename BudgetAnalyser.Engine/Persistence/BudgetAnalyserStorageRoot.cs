namespace BudgetAnalyser.Engine.Persistence
{
    public class BudgetAnalyserStorageRoot
    {
        public StorageBranch BudgetCollectionRootDto { get; set; }
        public StorageBranch LedgerBookRootDto { get; set; }
        public StorageBranch MatchingRulesCollectionRootDto { get; set; }
        public StorageBranch StatementModelRootDto { get; set; }
    }
}