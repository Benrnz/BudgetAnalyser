using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.Matching;

public class MatchingRuleEventArgs : EventArgs
{
    public required MatchingRule Rule { get; init; }
}
