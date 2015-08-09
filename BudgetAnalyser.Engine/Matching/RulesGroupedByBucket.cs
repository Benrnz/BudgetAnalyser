using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Matching
{
    /// <summary>
    ///     A class that models a group of matching rules grouped by a single <see cref="BudgetBucket" />.
    /// </summary>
    public class RulesGroupedByBucket 
    {
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

        public BudgetBucket Bucket { get; }
        public ObservableCollection<MatchingRule> Rules { get; }
        public int RulesCount => Rules.Count();
    }
}