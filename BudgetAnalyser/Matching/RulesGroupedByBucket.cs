using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.Matching
{
    public class RulesGroupedByBucket
    {
        public RulesGroupedByBucket([NotNull] BudgetBucket bucket, [NotNull] IEnumerable<MatchingRule> rules)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            Bucket = bucket;
            Rules = new BindingList<MatchingRule>(rules.ToList());
        }

        public BudgetBucket Bucket { get; private set; }

        public BindingList<MatchingRule> Rules { get; private set; }
    }
}