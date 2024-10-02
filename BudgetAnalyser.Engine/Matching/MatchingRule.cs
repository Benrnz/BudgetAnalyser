using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace BudgetAnalyser.Engine.Matching
{
    /// <summary>
    ///     An instance of this class describes how and if a transaction can be automatically matched to a Bucket when
    ///     auto-matching rules are applied.
    /// </summary>
    [DebuggerDisplay("Rule: {Description} {RuleId} {BucketCode}")]
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

        internal MatchingRule()
        {
            throw new NotSupportedException("This constructor is only used for producing mappers by reflection.");
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MatchingRule" /> class.
        /// </summary>
        /// <param name="bucketRepository">The bucket repository.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Reviewed, ok here")]
        public MatchingRule([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }

            this.bucketRepository = bucketRepository;
            RuleId = Guid.NewGuid();
            Created = DateTime.Now;
            And = true;
        }

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets or sets the amount criteria.
        ///     If null this field is not used to match.
        /// </summary>
        public decimal? Amount
        {
            get => this.doNotUseAmount;
            set
            {
                this.doNotUseAmount = value == 0 ? null : value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the criteria values are 'And'ed together or 'Or'ed.
        /// </summary>
        public bool And
        {
            get => this.doNotUseAnd;
            set
            {
                this.doNotUseAnd = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets the bucket to assign the transaction to if it matches the criteria specified in this rule.
        /// </summary>
        public BudgetBucket Bucket
        {
            get { return this.bucketRepository.GetByCode(BucketCode); }

            [UsedImplicitly]
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

        /// <summary>
        ///     Gets or sets the bucket code. Used only in persisting the rule. The <see cref="Bucket" /> property is lazy loaded
        ///     from this value.
        /// </summary>
        internal string BucketCode { get; set; }

        /// <summary>
        ///     Gets the date this rule was created.
        /// </summary>
        public DateTime Created { get; internal set; }

        /// <summary>
        ///     Gets or sets the description text to match.
        ///     If null this field is not used to match.
        /// </summary>
        public string Description
        {
            get => this.doNotUseDescription;
            set
            {
                this.doNotUseDescription = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this rule should be hidden in the UI. Also indicates this is a system rule.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        ///     Gets the last date and time the rule was matched to a transaction.
        /// </summary>
        public DateTime? LastMatch
        {
            get => this.doNotUseLastMatch;
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
            get => this.doNotUseMatchCount;
            internal set
            {
                this.doNotUseMatchCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the reference1 text to match.
        ///     If null this field is not used to match.
        /// </summary>
        public string Reference1
        {
            get => this.doNotUseReference1;

            set
            {
                this.doNotUseReference1 = value?.Trim();
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the reference2 text to match.
        ///     If null this field is not used to match.
        /// </summary>
        public string Reference2
        {
            get => this.doNotUseReference2;

            set
            {
                this.doNotUseReference2 = value?.Trim();
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the reference3 text to match.
        ///     If null this field is not used to match.
        /// </summary>
        public string Reference3
        {
            get => this.doNotUseReference3;

            set
            {
                this.doNotUseReference3 = value?.Trim();
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets the identifier that uniquely identifies this rule.
        /// </summary>
        public Guid RuleId { get; internal set; }

        /// <summary>
        ///     Gets or sets the transaction type text to match.
        ///     If null this field is not used to match.
        /// </summary>
        public string TransactionType
        {
            get => this.doNotUseTransactionType;
            set
            {
                this.doNotUseTransactionType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Returns true if the rule provided is this rule or the rule id's are equal.
        /// </summary>
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

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        ///     Delegates to <see cref="Equals(MatchingRule)" />.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
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
            return Equals((MatchingRule) obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode - Property setter is used by Persistence only
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
                throw new ArgumentNullException(nameof(transaction));
            }

            if (!Bucket.Active)
            {
                return false;
            }

            var matchesMade = 0;
            var totalComparisons = 0;
            if (!string.IsNullOrWhiteSpace(Description))
            {
                if (transaction.Description == Description)
                {
                    matchesMade++;
                }
                totalComparisons++;
            }

            if (!string.IsNullOrWhiteSpace(Reference1))
            {
                if (transaction.Reference1 == Reference1)
                {
                    matchesMade++;
                }
                totalComparisons++;
            }

            if (!string.IsNullOrWhiteSpace(Reference2))
            {
                if (transaction.Reference2 == Reference2)
                {
                    matchesMade++;
                }
                totalComparisons++;
            }

            if (!string.IsNullOrWhiteSpace(Reference3))
            {
                if (transaction.Reference3 == Reference3)
                {
                    matchesMade++;
                }
                totalComparisons++;
            }

            if (!string.IsNullOrWhiteSpace(TransactionType))
            {
                if (transaction.TransactionType.Name == TransactionType)
                {
                    matchesMade++;
                }
                totalComparisons++;
            }

            if (Amount != null)
            {
                if (transaction.Amount == Amount.Value)
                {
                    matchesMade++;
                }
                totalComparisons++;
            }

            var matched = And ? matchesMade == totalComparisons : matchesMade >= 1;
            if (matched)
            {
                LastMatch = DateTime.Now;
                MatchCount++;
            }

            return matched;
        }

        /// <summary>
        ///     Implements the operator ==. Delegates to Equals.
        /// </summary>
        public static bool operator ==(MatchingRule left, MatchingRule right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Implements the operator !=. Delegates to Equals.
        /// </summary>
        public static bool operator !=(MatchingRule left, MatchingRule right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}({1} {2} {3})", GetType().Name, Bucket.Code,
                                 Description ?? Reference1 ?? Reference2 ?? Reference3, Amount);
        }

        /// <summary>
        ///     Allows subclasses to set some protected and internal fields.
        /// </summary>
        protected void AllowSubclassAccess(string bucketCode, Guid ruleId)
        {
            BucketCode = bucketCode;
            RuleId = ruleId;
        }

        /// <summary>
        ///     Called when a property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}