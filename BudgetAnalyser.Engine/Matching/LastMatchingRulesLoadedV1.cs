using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Matching
{
    public class LastMatchingRulesLoadedV1 : IPersistent
    {
        public string MatchingRulesCollectionStorageKey { get; set; }

        public int LoadSequence { get { return 100; } }
    }
}