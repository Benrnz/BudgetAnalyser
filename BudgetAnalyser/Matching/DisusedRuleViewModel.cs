using System.Windows.Input;
using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.Matching;

public class DisusedRuleViewModel
{
    public required MatchingRule MatchingRule { get; init; }

    public required ICommand RemoveCommand { get; init; }
}
