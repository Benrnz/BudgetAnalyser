using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Statement
{
    public class StatementApplicationStateV1 : IPersistent
    {
        public int LoadSequence
        {
            get { return 20; }
        }

        public bool? SortByBucket { get; set; }
    }
}