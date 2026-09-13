using System.Collections.ObjectModel;
using System.Diagnostics;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Matching;

/// <summary>
///     A class that models a group of matching rules grouped by a single <see cref="BudgetBucket" />.
/// </summary>
[DebuggerDisplay("RulesGroupedByBucket: {Bucket.Code} {RulesCount} rules")]
public class RulesGroupedByBucket
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RulesGroupedByBucket" /> class.
    /// </summary>
    /// <param name="bucket">The bucket.</param>
    /// <param name="rules">The rules.</param>
    /// <exception cref="System.ArgumentNullException">
    /// </exception>
    public RulesGroupedByBucket(BudgetBucket bucket, IEnumerable<MatchingRule> rules)
    {
        if (rules is null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        Bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
        Rules = new ObservableCollection<MatchingRule>(rules.ToList());
    }

    /// <summary>
    ///     Gets the bucket for this group.
    /// </summary>
    public BudgetBucket Bucket { get; }

    /// <summary>
    ///     Gets the rules that match to this bucket.
    /// </summary>
    public ObservableCollection<MatchingRule> Rules { get; }

    /// <summary>
    ///     Gets the number of rules in this group.
    /// </summary>
    public int RulesCount => Rules.Count();
}
