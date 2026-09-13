using BudgetAnalyser.Engine.Matching;
using Rees.Wpf;

namespace BudgetAnalyser.Matching;

/// <summary>
///     A message broadcast when a new <see cref="MatchingRule" /> has been added to the rule list.
///     The <see cref="EditRulesUserControl" /> listens for this message to update its list boxes, which do not reflect
///     collection changes via data binding alone.
/// </summary>
public class RuleAddedMessage(MatchingRule rule) : MessageBase
{
    public MatchingRule Rule { get; } = rule;
}
