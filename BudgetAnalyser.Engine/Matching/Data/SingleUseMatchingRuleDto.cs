using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Matching.Data
{
    public class SingleUseMatchingRuleDto : MatchingRuleDto
    {
        [UsedImplicitly]
        public int Lifetime { get; set; }
    }
}