using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private BudgetBucket doNotUseBucket;
        private ObservableCollection<MatchingRule> rules;

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

            this.doNotUseBucket = bucket;
            Rules = new ObservableCollection<MatchingRule>(rules.ToList());
        }

        public BudgetBucket Bucket
        {
            get { return this.doNotUseBucket; }
            private set
            {
                this.doNotUseBucket = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MatchingRule> Rules
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