using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A special system ledger bucket to represent surplus funds available for surplus spending in the Ledger Book.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Ledger.LedgerBucket" />
    public class SurplusLedger : LedgerBucket
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SurplusLedger" /> class.
        /// </summary>
        public SurplusLedger()
        {
            BudgetBucket = new SurplusBucket();
        }

        /// <summary>
        ///     Allows ledger bucket specific behaviour during reconciliation.
        /// </summary>
        public override void ApplyReconciliationBehaviour(IList<LedgerTransaction> transactions, DateTime reconciliationDate,
                                                          decimal openingBalance)
        {
            // Nothing special to do here for Surplus.
        }

        /// <summary>
        ///     Validates the bucket provided is valid for use with this LedgerBucket. There is an explicit relationship between
        ///     <see cref="BudgetBucket" />s and <see cref="LedgerBucket" />s.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///     Invalid budget bucket used, only the Surplus bucket can be used with an
        ///     instance of Surplus-Ledger.
        /// </exception>
        protected override void ValidateBucketSet(BudgetBucket bucket)
        {
            if (bucket is SurplusBucket)
            {
                return;
            }

            throw new NotSupportedException(
                "Invalid budget bucket used, only the Surplus bucket can be used with an instance of Surplus-Ledger.");
        }
    }
}