using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Statement
{
    public class StatementApplicationStateV1 : IPersistent
    {
        public string StatementModelStorageKey { get; set; }

        public bool? SortByBucket { get; set; }

        public int LoadSequence { get { return 20; } }
    }
}