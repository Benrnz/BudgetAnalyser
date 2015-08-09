using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Matching
{
    /// <summary>
    ///     A class that models a group of matching rules grouped by a single <see cref="BudgetBucket" />.
    /// </summary>
    public class RulesGroupedByBucket : INotifyPropertyChanged
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

            this.Bucket = bucket;
            this.Rules = new ObservableCollection<MatchingRule>(rules.ToList());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public BudgetBucket Bucket { get; }
        public ObservableCollection<MatchingRule> Rules { get; }

        public int RulesCount
        {
            get { return Rules.Count(); }
        }

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