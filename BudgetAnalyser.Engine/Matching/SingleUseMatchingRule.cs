using System;
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
        public SingleUseMatchingRule([NotNull] IBudgetBucketRepository bucketRepository, int lifeTimeMatchTarget = 1) : base(bucketRepository)
        {
            if (lifeTimeMatchTarget <= 0) throw new ArgumentException($"Invalid value for '{nameof(lifeTimeMatchTarget)}' : {lifeTimeMatchTarget}, it must be 1 or greater.");
            Lifetime = lifeTimeMatchTarget;
            Hidden = true;
        }

        /// <summary>
        ///     The match count target. After reaching this target the rule will automatically be deleted.
        ///     Defaults to 1.
        /// </summary>
        public int Lifetime { get; set; }
    }
}