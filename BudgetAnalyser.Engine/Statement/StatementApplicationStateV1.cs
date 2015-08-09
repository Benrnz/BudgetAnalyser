using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Statement
{
    public class StatementApplicationStateV1 : IPersistent
    {
        public  int LoadSequence => 20;

        public bool? SortByBucket { get; set; }
    }
}