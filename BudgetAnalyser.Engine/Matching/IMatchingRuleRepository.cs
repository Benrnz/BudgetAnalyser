using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Matching
{
    public interface IMatchingRuleRepository
    {
        bool Exists(string storageKey);

        Task<IEnumerable<MatchingRule>> LoadRulesAsync(string storageKey);

        Task SaveRulesAsync(IEnumerable<MatchingRule> rules, string storageKey);
    }
}