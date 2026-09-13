using Rees.Wpf;

namespace BudgetAnalyser.Matching;

/// <summary>
///     A message broadcast when the sort order of the rules list has changed.
///     The <see cref="EditRulesUserControl" /> listens for this to refresh its list views.
/// </summary>
public class RuleSortChangedMessage : MessageBase
{
}
