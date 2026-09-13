using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Matching;

/// <summary>
///     A matching rule that is applied only once then deleted.
///     For example: Used to match system generated transactions with a unique reference code.
/// </summary>
public class SingleUseMatchingRule : MatchingRule
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SingleUseMatchingRule" /> class.
    /// </summary>
    /// <param name="bucketRepository">The bucket repository.</param>
    public SingleUseMatchingRule(IBudgetBucketRepository bucketRepository) : base(bucketRepository)
    {
        Hidden = true;
    }
}
