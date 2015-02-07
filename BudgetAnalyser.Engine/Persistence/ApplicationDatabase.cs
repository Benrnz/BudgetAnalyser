using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Persistence
{
    public class ApplicationDatabase
    {
        public string BudgetCollection { get; [UsedImplicitly] private set; }
        public string FileName { get; internal set; }
        public string LedgerBook { get; [UsedImplicitly] private set; }
        public string MatchingRulesCollection { get; [UsedImplicitly] private set; }
        public string StatementModel { get; [UsedImplicitly] private set; }
    }
}