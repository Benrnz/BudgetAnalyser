using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Matching
{
    public interface IMatchingRuleRepository
    {
        bool Exists(string fileName);
     
        IEnumerable<MatchingRule> LoadRules(string fileName);

        void SaveRules(IEnumerable<MatchingRule> rules, string fileName);
    }
}