using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

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
        /// <summary>
        ///     A constant for the "supplement less than budget" text
        /// </summary>
        protected const string SupplementLessThanBudgetText = "Automatically supplementing shortfall so balance is not less than monthly budget amount";

        /// <summary>
        ///     A constant for the "supplement overdrawn" text
        /// </summary>
        protected const string SupplementOverdrawnText = "Automatically supplementing overdrawn balance from surplus";

        private BudgetBucket budgetBucket;

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
        ///     A unique token to identify the bucket when using the code is not appropriate.
        /// </summary>
        public Guid Id { get; internal set; }

        /// <summary>
        ///     Gets or sets the Account in which this ledger's funds are stored.
        /// </summary>
        public Account StoredInAccount { get; internal set; }

        /// <summary>
        ///     Allows ledger bucket specific behaviour during reconciliation.
        /// </summary>
        public abstract void ApplyReconciliationBehaviour([NotNull] IList<LedgerTransaction> transactions, DateTime reconciliationDate, decimal openingBalance);

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        ///     Delegates to <see cref="Equals(LedgerBucket)" />
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
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
            return Equals((LedgerBucket) obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
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
        ///     Implements the operator ==. Delegates to Equals.
        /// </summary>
        public static bool operator ==(LedgerBucket left, LedgerBucket right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Implements the operator !=. Delegates to Equals.
        /// </summary>
        public static bool operator !=(LedgerBucket left, LedgerBucket right)
        {
            return !Equals(left, right);
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

        /// <summary>
        ///     Returns true if the to ledger buckets are refering to the same bucket.
        /// </summary>
        protected bool Equals([CanBeNull] LedgerBucket other)
        {
            if (other == null)
            {
                return false;
            }
            return Equals(BudgetBucket, other.BudgetBucket) && Equals(StoredInAccount, other.StoredInAccount);
        }

        /// <summary>
        ///     Validates the bucket provided is valid for use with this LedgerBucket. There is an explicit relationship between
        ///     <see cref="BudgetBucket" />s and <see cref="LedgerBucket" />s.
        /// </summary>
        protected abstract void ValidateBucketSet(BudgetBucket bucket);
    }
}