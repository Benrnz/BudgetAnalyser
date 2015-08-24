using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Matching
{
    /// <summary>
    ///     A matching rule that is applied only once then deleted.
    ///     For example: Used to match system generated transactions with a unique reference code.
    /// </summary>
    public class SingleUseMatchingRule : MatchingRule
    {
        public SingleUseMatchingRule([NotNull] IBudgetBucketRepository bucketRepository) : base(bucketRepository)
        {
            Hidden = true;
        }
    }
}