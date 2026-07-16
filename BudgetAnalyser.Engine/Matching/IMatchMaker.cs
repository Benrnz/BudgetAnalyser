using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.Matching;

internal interface IMatchmaker
{
    bool Match(IEnumerable<Transaction> transactions, IEnumerable<MatchingRule> rules);
}
