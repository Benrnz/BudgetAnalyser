using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
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
        public RulesGroupedByBucket([NotNull] BudgetBucket bucket, [NotNull] IEnumerable<MatchingRule> rules)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            Bucket = bucket;
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
}