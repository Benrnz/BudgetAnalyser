using BudgetAnalyser.Engine.Matching;
using Rees.Wpf;

namespace BudgetAnalyser.Matching;

/// <summary>
///     A message broadcast when a new <see cref="MatchingRule" /> has been created via the New Rule dialog.
///     The <see cref="AppliedRulesController" /> listens for this message to add the rule to the rule list.
/// </summary>
public class RuleCreatedMessage(MatchingRule rule) : MessageBase
{
    public MatchingRule Rule { get; } = rule;
}
