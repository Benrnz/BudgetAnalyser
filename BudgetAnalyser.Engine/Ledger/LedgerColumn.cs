using System.Diagnostics;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     Represents a column in a ledger.  A <see cref="LedgerColumn" /> tracks the balance of one
    ///     <see cref="BudgetBucket" /> and the
    ///     Bank Account in which it is stored.
    /// </summary>
    [DebuggerDisplay("Ledger({BudgetBucket})")]
    public class LedgerColumn
    {
        /// <summary>
        ///     Gets or sets the Budget Bucket this ledger column is tracking.
        /// </summary>
        public BudgetBucket BudgetBucket { get; internal set; }

        /// <summary>
        ///     Gets or sets the Account in which this ledger's funds are stored.
        /// </summary>
        public AccountType StoredInAccount { get; internal set; }

        public static bool operator ==(LedgerColumn left, LedgerColumn right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LedgerColumn left, LedgerColumn right)
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
            return Equals((LedgerColumn)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((BudgetBucket != null ? BudgetBucket.GetHashCode() : 0) * 397) ^ (StoredInAccount != null ? StoredInAccount.GetHashCode() : 0);
            }
        }

        protected bool Equals(LedgerColumn other)
        {
            return Equals(BudgetBucket, other.BudgetBucket) && Equals(StoredInAccount, other.StoredInAccount);
        }
    }
}