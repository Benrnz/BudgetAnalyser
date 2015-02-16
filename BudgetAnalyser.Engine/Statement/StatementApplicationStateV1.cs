using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Statement
{
    public class StatementApplicationStateV1 : IPersistent
    {
        public bool? SortByBucket { get; set; }

        public int LoadSequence { get { return 20; } }
    }
}