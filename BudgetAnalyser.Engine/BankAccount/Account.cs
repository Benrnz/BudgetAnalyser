using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.BankAccount
{
    /// <summary>
    ///     A type representing any bank account.
    /// </summary>
    public abstract class Account
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Account" /> class.
        /// </summary>
        protected Account()
        {
            Name = GetType().Name;
        }

        /// <summary>
        ///     Gets the type or classification of the account.
        /// </summary>
        public abstract AccountType AccountType { get; }

        /// <summary>
        ///     Gets the path to an image asset.
        /// </summary>
        [UsedImplicitly]
        public abstract string ImagePath { get; }

        /// <summary>
        ///     Gets a value indicating whether this account is an account where salaries are credited into.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is salary account; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSalaryAccount => false;

        /// <summary>
        ///     Gets or sets the name of the account.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance. Or if the
        ///     <see cref="Account" /> Equals the other <see cref="Account" />
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
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

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode - Name cannot be readonly because it is set implicitly by Automapper
            return Name?.GetHashCode() ?? 0;
        }

        /// <summary>
        ///     Implements the operator ==. Delegates to Equals.
        /// </summary>
        public static bool operator ==(Account left, Account right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Implements the operator !=. Delegates to Equals.
        /// </summary>
        public static bool operator !=(Account left, Account right)
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
            return Name + " Account";
        }

        /// <summary>
        ///     Returns true if the <see cref="Account.Name" /> is equal to the other <see cref="Account.Name" />
        /// </summary>
        protected bool Equals(Account? other)
        {
            if (other is null)
            {
                return false;
            }
            return string.Equals(Name, other.Name);
        }
    }
}