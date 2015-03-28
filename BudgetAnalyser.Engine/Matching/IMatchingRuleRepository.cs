using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    public interface IMatchingRuleRepository
    {
        Task CreateNewAsync([NotNull] string storageKey);
        Task<IEnumerable<MatchingRule>> LoadRulesAsync([NotNull] string storageKey);
        Task SaveRulesAsync([NotNull] IEnumerable<MatchingRule> rules, [NotNull] string storageKey);
    }
}