using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     A bank statement transaction.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.IComparable" />
    /// <seealso cref="BudgetAnalyser.Engine.ICloneable{Transaction}" />
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes",
        Justification =
            "IComparable is implemented for sorting only. One transactions is not considered < or > than another. Also Equals is not overiden."
        )]
    public class Transaction : INotifyPropertyChanged, IComparable, ICloneable<Transaction>
    {
        private BudgetBucket budgetBucket;
        private Account doNotUseAccount;
        private decimal doNotUseAmount;
        private DateTime doNotUseDate;
        private string doNotUseDescription;
        private string doNotUseReference1;
        private string doNotUseReference2;
        private string doNotUseReference3;
        private TransactionType doNotUseTransactionType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Transaction" /> class.
        /// </summary>
        public Transaction()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets or sets the bank account that this transaction belongs in.
        /// </summary>
        public Account Account
        {
            get { return this.doNotUseAccount; }
            set
            {
                this.doNotUseAccount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the transaction amount.
        /// </summary>
        public decimal Amount
        {
            get { return this.doNotUseAmount; }
            set
            {
                this.doNotUseAmount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the budget bucket classification for this transaction.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        ///     Setting a budget bucket to null when it already has a non-null value is
        ///     not allowed.
        /// </exception>
        public BudgetBucket BudgetBucket
        {
            get { return this.budgetBucket; }

            set
            {
                if (value == null && this.budgetBucket != null)
                {
                    throw new ArgumentNullException(nameof(value),
                        "Setting a budget bucket to null when it already has a non-null value is not allowed.");
                }

                this.budgetBucket = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the transaction date.
        /// </summary>
        public DateTime Date
        {
            get { return this.doNotUseDate; }
            set
            {
                this.doNotUseDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the transaction description.
        /// </summary>
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

        /// <summary>
        ///     Gets a value indicating whether this transaction is a suspected duplicate.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this transaction is suspected duplicate; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuspectedDuplicate { get; internal set; }

        /// <summary>
        ///     Gets or sets the transaction reference1.
        /// </summary>
        public string Reference1
        {
            get { return this.doNotUseReference1; }
            set
            {
                this.doNotUseReference1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the transaction reference2.
        /// </summary>
        public string Reference2
        {
            get { return this.doNotUseReference2; }
            set
            {
                this.doNotUseReference2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the transaction reference3.
        /// </summary>
        public string Reference3
        {
            get { return this.doNotUseReference3; }
            set
            {
                this.doNotUseReference3 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the type of the transaction. This is a type classification provided by the bank.
        /// </summary>
        public TransactionType TransactionType
        {
            get { return this.doNotUseTransactionType; }
            set
            {
                this.doNotUseTransactionType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Clones this transaction into a new instance.
        /// </summary>
        public Transaction Clone()
        {
            return new Transaction
            {
                Account = Account,
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

        /// <summary>
        ///     Compare the transaction to the one provided.
        /// </summary>
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
        ///     will give a different value (and it should) for every instance. If overriden changing hashcodes will cause problems
        ///     with
        ///     UI controls such as ListBox.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Following GetHashCode")]
        public int GetEqualityHashCode()
        {
            // WARNING: Do not add Bucket to this change detection.  It will interfer with finding transactions and duplicate detection.
            unchecked
            {
                var result = 37; // prime
                result += Account.GetType().GetHashCode();
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
            return string.Format(CultureInfo.CurrentUICulture, "Transaction: ({0} {1:N} {2} {3} {4} {5})", Date, Amount,
                Description, BudgetBucket.Code, Reference1, Id);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}