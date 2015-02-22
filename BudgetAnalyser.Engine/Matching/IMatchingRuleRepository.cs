using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Matching
{
    public interface IMatchingRuleRepository
    {
        bool Exists(string storageKey);

        IEnumerable<MatchingRule> LoadRules(string storageKey);

        void SaveRules(IEnumerable<MatchingRule> rules, string storageKey);
    }
}