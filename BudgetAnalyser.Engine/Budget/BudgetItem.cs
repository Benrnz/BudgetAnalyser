using System.ComponentModel;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    public abstract class BudgetItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual decimal Amount { get; set; }

        public BudgetBucket Bucket { get; set; }

        public static bool operator ==(BudgetItem left, BudgetItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BudgetItem left, BudgetItem right)
        {
            return !Equals(left, right);
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

            var other = obj as BudgetItem;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Bucket != null ? Bucket.GetHashCode() * GetType().GetHashCode() : 0);
            }
        }

        protected bool Equals(BudgetItem other)
        {
            return Equals(Bucket, other.Bucket) && GetType() == other.GetType();
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}