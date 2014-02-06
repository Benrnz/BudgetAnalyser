using System;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Matching
{
    [DebuggerDisplay("Rule: {Bucket} {Description} {Reference1} {Reference2} {Reference3}")]
    public class MatchingRule
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private decimal? doNotUseAmount;
        private string doNotUseReference1;
        private string doNotUseReference2;
        private string doNotUseReference3;

        /// <summary>
        ///     Used any other time.
        /// </summary>
        /// <param name="bucketRepository"></param>
        public MatchingRule([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.bucketRepository = bucketRepository;
        }

        public decimal? Amount
        {
            get { return this.doNotUseAmount; }
            set { this.doNotUseAmount = value == 0 ? null : value; }
        }

        public BudgetBucket Bucket
        {
            get { return this.bucketRepository.Buckets.FirstOrDefault(b => b.Id == BucketId); }

            set
            {
                if (value == null)
                {
                    BucketId = Guid.Empty;
                    return;
                }

                BucketId = value.Id;
            }
        }

        public string Description { get; set; }

        /// <summary>
        ///     Gets the last date and time the rule was matched to a transaction.
        /// </summary>
        public DateTime? LastMatch { get; internal set; }

        /// <summary>
        ///     Gets the number of times this rule has been matched to a transaction.
        /// </summary>
        public int MatchCount { get; internal set; }

        public string Reference1
        {
            get { return this.doNotUseReference1; }

            set { this.doNotUseReference1 = value == null ? null : value.Trim(); }
        }

        public string Reference2
        {
            get { return this.doNotUseReference2; }

            set { this.doNotUseReference2 = value == null ? null : value.Trim(); }
        }

        public string Reference3
        {
            get { return this.doNotUseReference3; }

            set { this.doNotUseReference3 = value == null ? null : value.Trim(); }
        }

        public string TransactionType { get; set; }
        internal Guid BucketId { get; set; }

        public bool Match(Transaction transaction)
        {
            bool matched = false;
            if (!string.IsNullOrWhiteSpace(Description))
            {
                if (transaction.Description == Description)
                {
                    matched = true;
                }
            }

            if (!matched && !string.IsNullOrWhiteSpace(Reference1))
            {
                if (transaction.Reference1 == Reference1)
                {
                    matched = true;
                }
            }

            if (!matched && !string.IsNullOrWhiteSpace(Reference2))
            {
                if (transaction.Reference2 == Reference2)
                {
                    matched = true;
                }
            }

            if (!matched && !string.IsNullOrWhiteSpace(Reference3))
            {
                if (transaction.Reference3 == Reference3)
                {
                    matched = true;
                }
            }

            if (!matched && !string.IsNullOrWhiteSpace(TransactionType))
            {
                if (transaction.TransactionType.Name == TransactionType)
                {
                    matched = true;
                }
            }

            if (matched)
            {
                LastMatch = DateTime.Now;
                MatchCount++;
            }

            return matched;
        }
    }
}