using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Matching
{
    [DebuggerDisplay("Rule: {Description} {RuleId} {BucketCode")]
    public class MatchingRule : INotifyPropertyChanged, IEquatable<MatchingRule>
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private decimal? doNotUseAmount;
        private bool doNotUseAnd;
        private string doNotUseDescription;
        private DateTime? doNotUseLastMatch;
        private int doNotUseMatchCount;
        private string doNotUseReference1;
        private string doNotUseReference2;
        private string doNotUseReference3;
        private string doNotUseTransactionType;

        /// <summary>
        ///     Used any other time.
        /// </summary>
        /// <param name="bucketRepository"></param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Reviewed, ok here")]
        public MatchingRule([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.bucketRepository = bucketRepository;
            RuleId = Guid.NewGuid();
            Created = DateTime.Now;
            And = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public decimal? Amount
        {
            get { return this.doNotUseAmount; }
            set
            {
                this.doNotUseAmount = value == 0 ? null : value;
                OnPropertyChanged();
            }
        }

        public bool And
        {
            get { return this.doNotUseAnd; }
            set
            {
                this.doNotUseAnd = value;
                OnPropertyChanged();
            }
        }

        public BudgetBucket Bucket
        {
            get { return this.bucketRepository.GetByCode(BucketCode); }

            private set
            {
                if (value == null)
                {
                    BucketCode = null;
                    return;
                }

                BucketCode = value.Code;
                OnPropertyChanged();
            }
        }

        public DateTime Created { get; internal set; }

        public string Description
        {
            get { return this.doNotUseDescription; }
            set
            {
                this.doNotUseDescription = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets the last date and time the rule was matched to a transaction.
        /// </summary>
        public DateTime? LastMatch
        {
            get { return this.doNotUseLastMatch; }
            internal set
            {
                this.doNotUseLastMatch = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets the number of times this rule has been matched to a transaction.
        /// </summary>
        public int MatchCount
        {
            get { return this.doNotUseMatchCount; }
            internal set
            {
                this.doNotUseMatchCount = value;
                OnPropertyChanged();
            }
        }

        public string Reference1
        {
            get { return this.doNotUseReference1; }

            set
            {
                this.doNotUseReference1 = value == null ? null : value.Trim();
                OnPropertyChanged();
            }
        }

        public string Reference2
        {
            get { return this.doNotUseReference2; }

            set
            {
                this.doNotUseReference2 = value == null ? null : value.Trim();
                OnPropertyChanged();
            }
        }

        public string Reference3
        {
            get { return this.doNotUseReference3; }

            set
            {
                this.doNotUseReference3 = value == null ? null : value.Trim();
                OnPropertyChanged();
            }
        }

        public Guid RuleId { get; internal set; }

        public string TransactionType
        {
            get { return this.doNotUseTransactionType; }
            set
            {
                this.doNotUseTransactionType = value;
                OnPropertyChanged();
            }
        }

        internal string BucketCode { get; set; }

        public static bool operator ==(MatchingRule left, MatchingRule right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MatchingRule left, MatchingRule right)
        {
            return !Equals(left, right);
        }

        public bool Equals(MatchingRule other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return RuleId.Equals(other.RuleId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((MatchingRule)obj);
        }

        public override int GetHashCode()
        {
            return RuleId.GetHashCode();
        }

        /// <summary>
        ///     Checks for a match with the given transactions.
        ///     All the properties on this rule are 'And'ed together.  This is an exact match search.
        /// </summary>
        /// <returns>true if a the rule matches the transactions</returns>
        public bool Match([NotNull] Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (!Bucket.Active)
            {
                return false;
            }

            var matchesMade = 0;
            var maxMatches = 0;
            if (!string.IsNullOrWhiteSpace(Description))
            {
                if (transaction.Description == Description)
                {
                    matchesMade++;
                }
                maxMatches++;
            }

            if (!string.IsNullOrWhiteSpace(Reference1))
            {
                if (transaction.Reference1 == Reference1)
                {
                    matchesMade++;
                }
                maxMatches++;
            }

            if (!string.IsNullOrWhiteSpace(Reference2))
            {
                if (transaction.Reference2 == Reference2)
                {
                    matchesMade++;
                }
                maxMatches++;
            }

            if (!string.IsNullOrWhiteSpace(Reference3))
            {
                if (transaction.Reference3 == Reference3)
                {
                    matchesMade++;
                }
                maxMatches++;
            }

            if (!string.IsNullOrWhiteSpace(TransactionType))
            {
                if (transaction.TransactionType.Name == TransactionType)
                {
                    matchesMade++;
                }
                maxMatches++;
            }

            if (Amount != null)
            {
                if (transaction.Amount == Amount.Value)
                {
                    matchesMade++;
                }
                maxMatches++;
            }

            bool matched = And ? matchesMade == maxMatches : matchesMade >= 1;
            if (matched)
            {
                LastMatch = DateTime.Now;
                MatchCount++;
            }

            return matched;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "MatchingRule({0} {1} {2})", Bucket.Code, Description, Amount);
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