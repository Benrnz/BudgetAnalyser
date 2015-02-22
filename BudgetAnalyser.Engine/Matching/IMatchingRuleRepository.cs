using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Matching
{
    public interface IMatchingRuleRepository
    {
        bool Exists(string storageKey);

        Task<IEnumerable<MatchingRule>> LoadRulesAsync(string storageKey);

        void SaveRules(IEnumerable<MatchingRule> rules, string storageKey);
    }
}