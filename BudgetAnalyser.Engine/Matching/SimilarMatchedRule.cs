using System;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    /// <summary>
    ///     Used for the purpose of showing similar rules as a check when creating a new rule.
    /// </summary>
    public class SimilarMatchedRule : MatchingRule
    {
        private bool doNotUseAmountMatched;
        private bool doNotUseDescriptionMatched;
        private bool doNotUseReference1Matched;
        private bool doNotUseReference2Matched;
        private bool doNotUseReference3Matched;
        private bool doNotUseTransactionTypeMatched;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SimilarMatchedRule" /> class.
        /// </summary>
        /// <param name="bucketRepository">The bucket repository.</param>
        /// <param name="rule">The rule.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public SimilarMatchedRule([NotNull] IBudgetBucketRepository bucketRepository, [NotNull] MatchingRule rule)
            : base(bucketRepository)
        {
            if (rule is null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            Amount = rule.Amount;
            And = rule.And;
            Description = rule.Description;
            Reference1 = rule.Reference1;
            Reference2 = rule.Reference2;
            Reference3 = rule.Reference3;
            TransactionType = rule.TransactionType;
            AllowSubclassAccess(rule.Bucket.Code, rule.RuleId);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the amount property was used to match.
        /// </summary>
        public bool AmountMatched
        {
            get { return this.doNotUseAmountMatched; }
            set
            {
                if (value == this.doNotUseAmountMatched)
                {
                    return;
                }
                this.doNotUseAmountMatched = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the description property was used to match.
        /// </summary>
        public bool DescriptionMatched
        {
            get { return this.doNotUseDescriptionMatched; }
            set
            {
                if (value == this.doNotUseDescriptionMatched)
                {
                    return;
                }
                this.doNotUseDescriptionMatched = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether reference1 property was used to match.
        /// </summary>
        public bool Reference1Matched
        {
            get { return this.doNotUseReference1Matched; }
            set
            {
                if (value == this.doNotUseReference1Matched)
                {
                    return;
                }
                this.doNotUseReference1Matched = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether reference2 property was used to match.
        /// </summary>
        public bool Reference2Matched
        {
            get { return this.doNotUseReference2Matched; }
            set
            {
                if (value == this.doNotUseReference2Matched)
                {
                    return;
                }
                this.doNotUseReference2Matched = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether reference3 property was used to match.
        /// </summary>
        public bool Reference3Matched
        {
            get { return this.doNotUseReference3Matched; }
            set
            {
                if (value == this.doNotUseReference3Matched)
                {
                    return;
                }
                this.doNotUseReference3Matched = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets the sort order.
        /// </summary>
        public int SortOrder
        {
            get
            {
                return (AmountMatched ? 100 : 0)
                       + (DescriptionMatched ? 90 : 0)
                       + (Reference1Matched ? 70 : 0)
                       + (Reference2Matched ? 60 : 0)
                       + (Reference3Matched ? 50 : 0)
                       + (TransactionTypeMatched ? 10 : 0);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the transaction type property was used to match.
        /// </summary>
        public bool TransactionTypeMatched
        {
            get { return this.doNotUseTransactionTypeMatched; }
            set
            {
                if (value == this.doNotUseTransactionTypeMatched)
                {
                    return;
                }
                this.doNotUseTransactionTypeMatched = value;
                OnPropertyChanged();
            }
        }
    }
}