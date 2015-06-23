using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Account
{
    public abstract class Account
    {
        protected Account()
        {
            Name = GetType().Name;
        }

        public abstract string ImagePath { get; }

        public string Name { get; protected set; }

        public abstract AccountType AccountType { get; }

        internal abstract string[] KeyWords { get; }

        public virtual bool IsSalaryAccount
        {
            get { return false; }
        }

        public static bool operator ==(Account left, Account right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Account left, Account right)
        {
            return !Equals(left, right);
        }

        public abstract Account Clone(string name);

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
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Name + " Account";
        }

        protected bool Equals([CanBeNull] Account other)
        {
            if (other == null) return false;
            return string.Equals(Name, other.Name);
        }
    }
}