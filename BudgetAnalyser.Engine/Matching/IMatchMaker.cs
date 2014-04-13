using System.Collections.Generic;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Matching
{
    public interface IMatchMaker
    {
        bool Match(IEnumerable<Transaction> transactions, IEnumerable<MatchingRule> rules);
    }
}