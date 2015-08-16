using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Account
{
    public abstract class Account
    {
        protected Account()
        {
            Name = GetType().Name;
        }

        public abstract AccountType AccountType { get; }

        [UsedImplicitly]
        public abstract string ImagePath { get; }

        public virtual bool IsSalaryAccount => false;
        public string Name { get; protected set; }

        public static bool operator ==(Account left, Account right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Account left, Account right)
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
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Account)obj);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode - Name cannot be readonly because it is set implicitly by Automapper
            return Name?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Name + " Account";
        }

        protected bool Equals([CanBeNull] Account other)
        {
            if (other == null)
            {
                return false;
            }
            return string.Equals(Name, other.Name);
        }
    }
}