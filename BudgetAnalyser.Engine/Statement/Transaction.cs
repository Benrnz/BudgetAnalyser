using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Statement
{
    [DebuggerDisplay("{Date} {Amount} {Description} {BudgetBucket}")]
    public class Transaction : INotifyPropertyChanged, IComparable, ICloneable
    {
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

        public Guid Id { get; set; }
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
                Id = Id,
                AccountType = AccountType,
                Amount = Amount,
                BudgetBucket = BudgetBucket,
                Date = Date,
                Description = Description,
                IsSuspectedDuplicate = IsSuspectedDuplicate,
                Reference1 = Reference1,
                Reference2 = Reference2,
                Reference3 = Reference3,
                TransactionType = TransactionType,
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

        public override int GetHashCode()
        {
            unchecked
            {
                int result = 37; // prime
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

                if (BudgetBucket != null)
                {
                    result += BudgetBucket.GetHashCode();
                    result *= 397;
                }

                return result;
            }
        }

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