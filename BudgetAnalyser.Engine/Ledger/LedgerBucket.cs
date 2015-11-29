using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     Represents a column in a ledger.  A <see cref="LedgerBucket" /> tracks the balance of one
    ///     <see cref="BudgetBucket" /> and the
    ///     Bank Account in which it is stored.
    /// </summary>
    [DebuggerDisplay("Ledger({BudgetBucket})")]
    public abstract class LedgerBucket
    {
        private BudgetBucket budgetBucket;
        protected const string SupplementOverdrawnText = "Automatically supplementing overdrawn balance from surplus";
        protected const string SupplementLessThanBudgetText = "Automatically supplementing shortfall so balance is not less than monthly budget amount";
        protected const string RemoveExcessNoBudgetAmountText = "Automatically removing excess funds down to zero given there is no budget amount for this ledger";
        protected const string RemoveExcessText = "Automatically removing excess funds.";

        /// <summary>
        ///     Gets or sets the Budget Bucket this ledger column is tracking.
        /// </summary>
        public BudgetBucket BudgetBucket
        {
            get { return this.budgetBucket; }
            internal set
            {
                ValidateBucketSet(value);
                this.budgetBucket = value;
            }
        }

        /// <summary>
        ///     Gets or sets the Account in which this ledger's funds are stored.
        /// </summary>
        public Account StoredInAccount { get; internal set; }

        public static bool operator ==(LedgerBucket left, LedgerBucket right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LedgerBucket left, LedgerBucket right)
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
            return Equals((LedgerBucket)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode - Properties are only set by persistence
                return ((BudgetBucket?.GetHashCode() ?? 0) * 397) ^ (StoredInAccount?.GetHashCode() ?? 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
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
            return string.Format(CultureInfo.CurrentCulture, "Ledger Bucket {0}", BudgetBucket);
        }

        public abstract void ReconciliationBehaviour(List<LedgerTransaction> transactions, DateTime reconciliationDate, decimal openingBalance);

        public abstract void ValidateBucketSet(BudgetBucket bucket);

        protected bool Equals([CanBeNull] LedgerBucket other)
        {
            if (other == null)
            {
                return false;
            }
            return Equals(BudgetBucket, other.BudgetBucket) && Equals(StoredInAccount, other.StoredInAccount);
        }
    }
}