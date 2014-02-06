using System.ComponentModel;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    public abstract class BudgetItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual decimal Amount { get; set; }

        public virtual BudgetBucket Bucket { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var otherBudget = obj as BudgetItem;
            if (otherBudget == null)
            {
                return false;
            }

            return Bucket.Code == otherBudget.Bucket.Code;
        }

        public override int GetHashCode()
        {
            return Bucket.Code.GetHashCode();
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