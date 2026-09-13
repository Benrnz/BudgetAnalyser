using BudgetAnalyser.Engine.Matching;
using Rees.Wpf;

namespace BudgetAnalyser.Matching;

/// <summary>
///     A message broadcast when a <see cref="MatchingRule" /> has been removed from the rule list.
///     The <see cref="EditRulesUserControl" /> listens for this message to update its list boxes, which do not reflect
///     collection changes via data binding alone.
/// </summary>
public class RuleRemovedMessage(MatchingRule rule) : MessageBase
{
    public MatchingRule Rule { get; } = rule;
}
