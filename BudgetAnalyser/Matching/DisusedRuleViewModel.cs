using System.Windows.Input;
using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.Matching
{
    public class DisusedRuleViewModel
    {
        public MatchingRule MatchingRule { get; set; }

        public ICommand RemoveCommand { get; set; }
    }
}
