using System.Diagnostics;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    /// Represents a column in a ledger.  A <see cref="LedgerColumn"/> tracks the balance of one <see cref="BudgetBucket"/>.
    /// </summary>
    [DebuggerDisplay("Ledger({BudgetBucket})")]
    public class LedgerColumn
    {
        /// <summary>
        /// Gets or sets the Budget Bucket this ledger column is tracking.
        /// </summary>
        public BudgetBucket BudgetBucket { get; internal set; }

        /// <summary>
        /// Gets or sets the Account in which this ledger's funds are stored.
        /// </summary>
        public AccountType StoredInAccount { get; internal set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherLedger = obj as LedgerColumn;
            if (otherLedger == null)
            {
                return false;
            }

            return BudgetBucket.Code.Equals(otherLedger.BudgetBucket.Code);
        }

        public override int GetHashCode()
        {
            int hash = BudgetBucket.Code.GetHashCode();
            return hash;
        }
    }
}