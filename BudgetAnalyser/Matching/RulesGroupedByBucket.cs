using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.Matching
{
    /// <summary>
    /// A class that models a group of matching rules grouped by a single <see cref="BudgetBucket"/>.
    /// </summary>
    public class RulesGroupedByBucket : INotifyPropertyChanged
    {
        private BudgetBucket bucket;
        private BindingList<MatchingRule> rules;

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

        public BudgetBucket Bucket
        {
            get { return this.bucket; }
            private set
            {
                this.bucket = value;
                OnPropertyChanged();
            }
        }

        public BindingList<MatchingRule> Rules
        {
            get { return this.rules; }
            private set
            {
                this.rules = value;
                OnPropertyChanged();
            }
        }

        public int RulesCount
        {
            get { return Rules.Count(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}