using System;
using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.Matching
{
    public class MatchingRuleEventArgs : EventArgs
    {
        public MatchingRule Rule { get; set; }
    }
}
