using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Statement
{
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes",
        Justification = "IComparable is implemented for sorting only. One transactions is not considered < or > than another. Also Equals is not overiden.")]
    public class Transaction : INotifyPropertyChanged, IComparable, ICloneable
    {
        public const string AmountPropertyName = "Amount";
        public const string BucketPropertyName = "BudgetBucket";
        public const string DatePropertyName = "Date";
        private BudgetBucket budgetBucket;
        private AccountType doNotUseAccountType;
        private decimal doNotUseAmount;
        private DateTime doNotUseDate;
        private string doNotUseDescription;
        private string doNotUseReference1;
        private string doNotUseReference2;
        private string doNotUseReference3;
        private TransactionType doNotUseTransactionType;

        public Transaction()
        {
            Id = Guid.NewGuid();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AccountType AccountType
        {
            get { return this.doNotUseAccountType; }
            set
            {
                this.doNotUseAccountType = value;
                OnPropertyChanged();
            }
        }

        public decimal Amount
        {
            get { return this.doNotUseAmount; }
            set
            {
                this.doNotUseAmount = value;
                OnPropertyChanged();
            }
        }

        public BudgetBucket BudgetBucket
        {
            get { return this.budgetBucket; }

            set
            {
                if (value == null && this.budgetBucket != null)
                {
                    throw new ArgumentNullException("value", "Setting a budget bucket to null when it already has a non-null value is not allowed.");
                }

                this.budgetBucket = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get { return this.doNotUseDate; }
            set
            {
                this.doNotUseDate = value;
                OnPropertyChanged();
            }
        }

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
        ///     The unique identifier for the transaction.  Ideally this should not be public settable, but this is used during
        ///     serialisation.
        /// </summary>
        public Guid Id { get; internal set; }

        public bool IsSuspectedDuplicate { get; internal set; }

        public string Reference1
        {
            get { return this.doNotUseReference1; }
            set
            {
                this.doNotUseReference1 = value;
                OnPropertyChanged();
            }
        }

        public string Reference2
        {
            get { return this.doNotUseReference2; }
            set
            {
                this.doNotUseReference2 = value;
                OnPropertyChanged();
            }
        }

        public string Reference3
        {
            get { return this.doNotUseReference3; }
            set
            {
                this.doNotUseReference3 = value;
                OnPropertyChanged();
            }
        }

        public TransactionType TransactionType
        {
            get { return this.doNotUseTransactionType; }
            set
            {
                this.doNotUseTransactionType = value;
                OnPropertyChanged();
            }
        }

        public object Clone()
        {
            return new Transaction
            {
                AccountType = AccountType,
                Amount = Amount,
                BudgetBucket = BudgetBucket,
                Date = Date,
                Description = Description,
                Reference1 = Reference1,
                Reference2 = Reference2,
                Reference3 = Reference3,
                TransactionType = TransactionType
            };
        }

        public int CompareTo(object obj)
        {
            var otherTransaction = obj as Transaction;
            if (otherTransaction == null)
            {
                return 1;
            }

            return Date.CompareTo(otherTransaction.Date);
        }

        /// <summary>
        ///     Get a hash code that will indicate value based equivalence with another instance of <see cref="Transaction" />.
        ///     <see cref="Object.GetHashCode" /> cannot be used because it is intended to show instance reference equivalence. It
        ///     will
        ///     give a different value (and it should) for every instance. If overriden changing hashcodes will cause problems with
        ///     UI controls such as ListBox.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Following GetHashCode")]
        public int GetEqualityHashCode()
        {
            unchecked
            {
                var result = 37; // prime
                result += AccountType.GetType().GetHashCode();
                result *= 397; // also prime 
                result += Amount.GetHashCode();
                result *= 397;
                result += Date.GetHashCode();
                result *= 397;

                if (!string.IsNullOrWhiteSpace(Description))
                {
                    result += Description.GetHashCode();
                }
                result *= 397;

                if (!string.IsNullOrWhiteSpace(Reference1))
                {
                    result += Reference1.GetHashCode();
                }
                result *= 397;

                if (!string.IsNullOrWhiteSpace(Reference2))
                {
                    result += Reference2.GetHashCode();
                }
                result *= 397;

                if (!string.IsNullOrWhiteSpace(Reference3))
                {
                    result += Reference3.GetHashCode();
                }
                result *= 397;

                result += TransactionType.GetHashCode();
                result *= 397;

                return result;
            }
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Transaction: ({0} {1:N} {2} {3} {4} {5})", Date, Amount, Description, BudgetBucket.Code, Reference1, Id);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}