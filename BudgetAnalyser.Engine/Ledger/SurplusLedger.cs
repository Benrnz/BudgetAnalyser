using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    public class SurplusLedger : LedgerBucket
    {
        public SurplusLedger()
        {
            BudgetBucket = new SurplusBucket();
        }

        public override void ReconciliationBehaviour(IList<LedgerTransaction> transactions, DateTime reconciliationDate, decimal openingBalance)
        {
            // Nothing special to do here for Surplus.
        }

        public override void ValidateBucketSet(BudgetBucket bucket)
        {
            if (bucket is SurplusBucket)
            {
                return;
            }

            throw new NotSupportedException("Invalid budget bucket used, only the Surplus bucket can be used with an instance of Surplus-Ledger.");
        }
    }
}